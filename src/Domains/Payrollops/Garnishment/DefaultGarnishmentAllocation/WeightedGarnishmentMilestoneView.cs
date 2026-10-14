namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted garnishment milestone.</summary>
public sealed record WeightedGarnishmentMilestoneView(string Code, string Title, string ArchivedText, string Segment, string SalaryBand, string Quantity, string StatusLabel)
{
    public static WeightedGarnishmentMilestoneView ToWeightedGarnishmentMilestoneView(WeightedGarnishmentMilestone item)
    {
        return new WeightedGarnishmentMilestoneView(
            Text.Truncate(item.Code, 80),
            item.Title,
            $"{item.Archived}",
            Text.Truncate(item.Segment, 80),
            Text.Truncate(item.SalaryBand, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedGarnishmentMilestoneView> ToWeightedGarnishmentMilestoneViews(List<WeightedGarnishmentMilestone> items)
    {
        var views = new List<WeightedGarnishmentMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedGarnishmentMilestoneView(item));
        }
        return views;
    }

    public static double SumWeightedGarnishmentMilestoneView(WeightedGarnishmentMilestoneView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
