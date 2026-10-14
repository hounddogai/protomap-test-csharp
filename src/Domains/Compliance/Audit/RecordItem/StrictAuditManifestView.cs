namespace Acme.Workforce.Compliance.Audit.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict audit manifest.</summary>
public sealed record StrictAuditManifestView(string Code, string Title, string Active, string Archived, string Minimum, string StatusLabel)
{
    public static StrictAuditManifestView ToStrictAuditManifestView(StrictAuditManifest item)
    {
        return new StrictAuditManifestView(
            Text.Truncate(item.Code, 80),
            item.Title,
            $"{item.Active}",
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAuditManifestView> ToStrictAuditManifestViews(List<StrictAuditManifest> items)
    {
        var views = new List<StrictAuditManifestView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAuditManifestView(item));
        }
        return views;
    }

    public static double SumStrictAuditManifestView(StrictAuditManifestView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
