namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal audit claim.</summary>
public sealed record SeasonalAuditClaimView(string Code, string Label, string Amount, string Country, string Visible, string JobCode, string StatusLabel)
{
    public static SeasonalAuditClaimView ToSeasonalAuditClaimView(SeasonalAuditClaim item)
    {
        return new SeasonalAuditClaimView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Visible}",
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAuditClaimView> ToSeasonalAuditClaimViews(List<SeasonalAuditClaim> items)
    {
        var views = new List<SeasonalAuditClaimView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAuditClaimView(item));
        }
        return views;
    }

    public static double SumSeasonalAuditClaimView(SeasonalAuditClaimView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
