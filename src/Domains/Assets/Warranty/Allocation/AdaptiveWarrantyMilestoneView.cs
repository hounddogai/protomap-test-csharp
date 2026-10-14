namespace Acme.Workforce.Assets.Warranty.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive warranty milestone.</summary>
public sealed record AdaptiveWarrantyMilestoneView(string Code, string Label, string Limit, string Checksum, string Unit, string Budget, string CostCenter, string WeightText, string AmountText, string StatusLabel)
{
    public static AdaptiveWarrantyMilestoneView ToAdaptiveWarrantyMilestoneView(AdaptiveWarrantyMilestone item)
    {
        return new AdaptiveWarrantyMilestoneView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Unit,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveWarrantyMilestoneView> ToAdaptiveWarrantyMilestoneViews(List<AdaptiveWarrantyMilestone> items)
    {
        var views = new List<AdaptiveWarrantyMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveWarrantyMilestoneView(item));
        }
        return views;
    }

    public static double SumAdaptiveWarrantyMilestoneView(AdaptiveWarrantyMilestoneView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedBudget + parsedWeight;
    }
}
