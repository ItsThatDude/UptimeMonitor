namespace UptimeMonitor.Core.Contracts.Api.System
{
    public record AdminUpdateSystemSettingsRequest(
        string? DefaultStatusPageSlug
    );
}