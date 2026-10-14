namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual action plan preference.</summary>
public sealed record ManualActionPlanPreferenceView(string Code, string Name, string BatchSize, string Checksum, string DepartmentText, string Threshold, string QuantityText, string RatioText, string StatusLabel)
{
    public static ManualActionPlanPreferenceView ToManualActionPlanPreferenceView(ManualActionPlanPreference item)
    {
        return new ManualActionPlanPreferenceView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            item.Department,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualActionPlanPreferenceView> ToManualActionPlanPreferenceViews(List<ManualActionPlanPreference> items)
    {
        var views = new List<ManualActionPlanPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToManualActionPlanPreferenceView(item));
        }
        return views;
    }

    public static double SumManualActionPlanPreferenceView(ManualActionPlanPreferenceView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedThreshold + (double)parsedQuantity;
    }
}
