namespace Acme.Workforce.Compliance.Audit.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy audit claim.</summary>
public sealed record LegacyAuditClaimView(string Code, string Title, string RevisionText, string SalaryBand, string OwnerId, string Archived, string UpdatedAt, string StatusLabel)
{
    public static LegacyAuditClaimView ToLegacyAuditClaimView(LegacyAuditClaim item)
    {
        return new LegacyAuditClaimView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.OwnerId, 120),
            $"{item.Archived}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAuditClaimView> ToLegacyAuditClaimViews(List<LegacyAuditClaim> items)
    {
        var views = new List<LegacyAuditClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAuditClaimView(item));
        }
        return views;
    }

    public static double SumLegacyAuditClaimView(LegacyAuditClaimView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedUpdatedAt;
    }
}
