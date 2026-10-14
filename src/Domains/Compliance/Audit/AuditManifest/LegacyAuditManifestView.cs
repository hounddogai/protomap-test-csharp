namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy audit manifest.</summary>
public sealed record LegacyAuditManifestView(string Code, string NameText, string EmployeeIdText, string Retries, string DurationMinutes, string Attempts, string Capacity, string Team, string FteText, string StatusLabel)
{
    public static LegacyAuditManifestView ToLegacyAuditManifestView(LegacyAuditManifest item)
    {
        return new LegacyAuditManifestView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 80),
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAuditManifestView> ToLegacyAuditManifestViews(List<LegacyAuditManifest> items)
    {
        var views = new List<LegacyAuditManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAuditManifestView(item));
        }
        return views;
    }

    public static double SumLegacyAuditManifestView(LegacyAuditManifestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedDurationMinutes + (double)parsedAttempts;
    }
}
