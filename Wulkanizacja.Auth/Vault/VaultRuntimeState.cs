namespace Wulkanizacja.Auth.Vault;

public sealed class VaultRuntimeState
{
    public bool Enabled { get; init; }
    public string? Address { get; init; }
    public string? Token { get; set; }
    public bool TokenRenewable { get; set; }
    public DateTimeOffset? NextRenewalUtc { get; set; }
}
