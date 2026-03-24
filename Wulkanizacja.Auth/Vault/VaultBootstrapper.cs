using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Wulkanizacja.Auth.Vault;

public static class VaultBootstrapper
{
    public static async Task<VaultRuntimeState> LoadSecretsAsync(ConfigurationManager configuration, CancellationToken cancellationToken = default)
    {
        var enabled = configuration.GetValue<bool?>("Vault:Enabled") ?? false;
        if (!enabled)
        {
            return new VaultRuntimeState { Enabled = false };
        }

        var address = configuration["Vault:Address"];
        var secretPath = configuration["Vault:SecretPath"];
        var authPath = configuration["Vault:AuthPath"] ?? "auth/approle/login";

        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(secretPath))
        {
            throw new InvalidOperationException("Vault jest wlaczony, ale brakuje Vault:Address lub Vault:SecretPath.");
        }

        using var client = new HttpClient { BaseAddress = new Uri(address) };

        var token = configuration["Vault:Token"];
        var tokenRenewable = false;
        var nextRenewalUtc = (DateTimeOffset?)null;

        if (string.IsNullOrWhiteSpace(token))
        {
            var roleId = configuration["Vault:RoleId"];
            var secretId = configuration["Vault:SecretId"];

            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(secretId))
            {
                throw new InvalidOperationException("Brak Vault:Token oraz AppRole (Vault:RoleId, Vault:SecretId).");
            }

            var loginPayload = JsonSerializer.Serialize(new { role_id = roleId, secret_id = secretId });
            using var loginResponse = await client.PostAsync($"/v1/{authPath.TrimStart('/')}", new StringContent(loginPayload, Encoding.UTF8, "application/json"), cancellationToken);

            loginResponse.EnsureSuccessStatusCode();

            using var loginDoc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync(cancellationToken));
            var authElement = loginDoc.RootElement.GetProperty("auth");

            token = authElement.GetProperty("client_token").GetString();
            tokenRenewable = authElement.TryGetProperty("renewable", out var renewableEl) && renewableEl.GetBoolean();
            if (tokenRenewable && authElement.TryGetProperty("lease_duration", out var leaseEl))
            {
                nextRenewalUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(30, (int)(leaseEl.GetInt32() * 0.7)));
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Nie udalo sie uzyskac tokenu Vault.");
        }

        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Add("X-Vault-Token", token);

        using var secretResponse = await client.GetAsync($"/v1/secret/data/{secretPath}", cancellationToken);
        secretResponse.EnsureSuccessStatusCode();

        using var secretDoc = JsonDocument.Parse(await secretResponse.Content.ReadAsStringAsync(cancellationToken));
        var dataObject = secretDoc.RootElement.GetProperty("data").GetProperty("data");

        var loaded = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in dataObject.EnumerateObject())
        {
            var key = property.Name.Replace("__", ":", StringComparison.Ordinal);
            var value = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.ToString();

            loaded[key] = value;
        }

        configuration.AddInMemoryCollection(loaded);

        return new VaultRuntimeState
        {
            Enabled = true,
            Address = address,
            Token = token,
            TokenRenewable = tokenRenewable,
            NextRenewalUtc = nextRenewalUtc
        };
    }
}
