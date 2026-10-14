namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary succession plan variance.</summary>
public sealed record PrimarySuccessionPlanVarianceView(string CodeText, string Title, string Threshold, string Capacity, string UpdatedAt, string ActiveText, string ArchivedText, string Rate, string QuantityText, string UnitText, string MarginText, string StatusLabel)
{
    public static PrimarySuccessionPlanVarianceView ToPrimarySuccessionPlanVarianceView(PrimarySuccessionPlanVariance item)
    {
        return new PrimarySuccessionPlanVarianceView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Archived}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimarySuccessionPlanVarianceView> ToPrimarySuccessionPlanVarianceViews(List<PrimarySuccessionPlanVariance> items)
    {
        var views = new List<PrimarySuccessionPlanVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimarySuccessionPlanVarianceView(item));
        }
        return views;
    }

    public static double SumPrimarySuccessionPlanVarianceView(PrimarySuccessionPlanVarianceView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedCapacity + (double)parsedUpdatedAt;
    }
}
