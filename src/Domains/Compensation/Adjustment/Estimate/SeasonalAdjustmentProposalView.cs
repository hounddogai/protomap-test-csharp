namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal adjustment proposal.</summary>
public sealed record SeasonalAdjustmentProposalView(string Code, string LabelText, string Quantity, string Hours, string JobCode, string Title, string OwnerId, string DiscountText, string Timezone, string Badge, string StatusLabel)
{
    public static SeasonalAdjustmentProposalView ToSeasonalAdjustmentProposalView(SeasonalAdjustmentProposal item)
    {
        return new SeasonalAdjustmentProposalView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Title,
            Text.Truncate(item.OwnerId, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAdjustmentProposalView> ToSeasonalAdjustmentProposalViews(List<SeasonalAdjustmentProposal> items)
    {
        var views = new List<SeasonalAdjustmentProposalView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAdjustmentProposalView(item));
        }
        return views;
    }

    public static double SumSeasonalAdjustmentProposalView(SeasonalAdjustmentProposalView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedHours + parsedDiscount;
    }
}
