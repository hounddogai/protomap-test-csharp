namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit claim.</summary>
public sealed record AuditClaimView(string Code, string Name, string RetriesText, string StartsAt, string UpdatedAtText, string StatusLabel)
{
    public static AuditClaimView ToAuditClaimView(AuditClaim item)
    {
        return new AuditClaimView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditClaimView> ToAuditClaimViews(List<AuditClaim> items)
    {
        var views = new List<AuditClaimView>();
        foreach (var item in items)
        {
            views.Add(ToAuditClaimView(item));
        }
        return views;
    }

    public static double SumAuditClaimView(AuditClaimView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedStartsAt + (double)parsedUpdatedAt;
    }
}
