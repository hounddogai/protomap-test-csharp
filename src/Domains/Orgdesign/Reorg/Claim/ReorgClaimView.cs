namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg claim.</summary>
public sealed record ReorgClaimView(string Code, string Title, string Team, string Minimum, string CostCenter, string Archived, string SegmentText, string BudgetText, string Badge, string EndsAt, string FloorValue, string StatusLabel)
{
    public static ReorgClaimView ToReorgClaimView(ReorgClaim item)
    {
        return new ReorgClaimView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Team,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Archived}",
            item.Segment,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgClaimView> ToReorgClaimViews(List<ReorgClaim> items)
    {
        var views = new List<ReorgClaimView>();
        foreach (var item in items)
        {
            views.Add(ToReorgClaimView(item));
        }
        return views;
    }

    public static double SumReorgClaimView(ReorgClaimView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget + (double)parsedEndsAt;
    }
}
