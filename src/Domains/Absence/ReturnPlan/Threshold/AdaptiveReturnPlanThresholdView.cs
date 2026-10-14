namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive return plan threshold.</summary>
public sealed record AdaptiveReturnPlanThresholdView(string Code, string Label, string ExpiresAt, string DepartmentText, string Priority, string CapacityText, string ApprovedText, string Margin, string NotesText, string Rating, string Category, string StatusLabel)
{
    public static AdaptiveReturnPlanThresholdView ToAdaptiveReturnPlanThresholdView(AdaptiveReturnPlanThreshold item)
    {
        return new AdaptiveReturnPlanThresholdView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveReturnPlanThresholdView> ToAdaptiveReturnPlanThresholdViews(List<AdaptiveReturnPlanThreshold> items)
    {
        var views = new List<AdaptiveReturnPlanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveReturnPlanThresholdView(item));
        }
        return views;
    }

    public static double SumAdaptiveReturnPlanThresholdView(AdaptiveReturnPlanThresholdView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedPriority + (double)parsedCapacity;
    }
}
