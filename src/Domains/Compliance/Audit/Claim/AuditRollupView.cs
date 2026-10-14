namespace Acme.Workforce.Compliance.Audit.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit rollup.</summary>
public sealed record AuditRollupView(string Code, string Title, string Minimum, string UpdatedAt, string Revision, string Active, string StartsAt, string Category, string CeilingText, string Margin, string StatusLabel)
{
    public static AuditRollupView ToAuditRollupView(AuditRollup item)
    {
        return new AuditRollupView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditRollupView> ToAuditRollupViews(List<AuditRollup> items)
    {
        var views = new List<AuditRollupView>();
        foreach (var item in items)
        {
            views.Add(ToAuditRollupView(item));
        }
        return views;
    }

    public static double SumAuditRollupView(AuditRollupView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedUpdatedAt + (double)parsedRevision;
    }
}
