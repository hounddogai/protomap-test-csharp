namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pay run milestone.</summary>
public sealed record TieredPayRunMilestoneView(string Code, string Label, string TenureMonths, string RateText, string Threshold, string Attempts, string CapacityText, string Visible, string StatusLabel)
{
    public static TieredPayRunMilestoneView ToTieredPayRunMilestoneView(TieredPayRunMilestone item)
    {
        return new TieredPayRunMilestoneView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPayRunMilestoneView> ToTieredPayRunMilestoneViews(List<TieredPayRunMilestone> items)
    {
        var views = new List<TieredPayRunMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPayRunMilestoneView(item));
        }
        return views;
    }

    public static double SumTieredPayRunMilestoneView(TieredPayRunMilestoneView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedRate + parsedThreshold;
    }
}
