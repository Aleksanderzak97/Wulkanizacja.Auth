using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wulkanizacja.Auth.Vault;

public sealed class VaultTokenRefreshService(VaultRuntimeState state, ILogger<VaultTokenRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!state.Enabled || string.IsNullOrWhiteSpace(state.Address) || string.IsNullOrWhiteSpace(state.Token))
        {
            logger.LogInformation("Vault token refresh jest wylaczony.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!state.TokenRenewable || state.NextRenewalUtc is null)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }

            var delay = state.NextRenewalUtc.Value - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.FromSeconds(5))
            {
                delay = TimeSpan.FromSeconds(5);
            }

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri(state.Address) };
                client.DefaultRequestHeaders.Add("X-Vault-Token", state.Token);

                using var response = await client.PostAsync("/v1/auth/token/renew-self", new StringContent("{}", Encoding.UTF8, "application/json"), stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    state.TokenRenewable = false;
                    logger.LogWarning("Nie udalo sie odswiezyc tokenu Vault (HTTP {Status}).", (int)response.StatusCode);
                    continue;
                }

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(stoppingToken));
                var auth = document.RootElement.GetProperty("auth");
                state.TokenRenewable = auth.TryGetProperty("renewable", out var renewableEl) && renewableEl.GetBoolean();

                if (state.TokenRenewable && auth.TryGetProperty("lease_duration", out var leaseEl))
                {
                    state.NextRenewalUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(30, (int)(leaseEl.GetInt32() * 0.7)));
                }
                else
                {
                    state.NextRenewalUtc = null;
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Blad podczas odswiezania tokenu Vault.");
                state.NextRenewalUtc = DateTimeOffset.UtcNow.AddMinutes(1);
            }
        }
    }
}
