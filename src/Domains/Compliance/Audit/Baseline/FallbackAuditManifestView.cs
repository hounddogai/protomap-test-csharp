namespace Acme.Workforce.Compliance.Audit.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a fallback audit manifest.</summary>
public sealed record FallbackAuditManifestView(string Code, string Label, string LocaleText, string CreatedAt, string PinnedText, string Grade, string Team, string Source, string Unit, string ExpiresAtText, string Channel, string StatusLabel)
{
    public static FallbackAuditManifestView ToFallbackAuditManifestView(FallbackAuditManifest item)
    {
        return new FallbackAuditManifestView(
            item.Code,
            item.Label,
            item.Locale,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Grade,
            item.Team,
            item.Source,
            item.Unit,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAuditManifestView> ToFallbackAuditManifestViews(List<FallbackAuditManifest> items)
    {
        var views = new List<FallbackAuditManifestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAuditManifestView(item));
        }
        return views;
    }

    public static double SumFallbackAuditManifestView(FallbackAuditManifestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedExpiresAt;
    }
}
