namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive occupancy template.</summary>
public sealed record AdaptiveOccupancyTemplateView(string CodeText, string Label, string Source, string Threshold, string CostCenter, string Department, string StatusLabel)
{
    public static AdaptiveOccupancyTemplateView ToAdaptiveOccupancyTemplateView(AdaptiveOccupancyTemplate item)
    {
        return new AdaptiveOccupancyTemplateView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Department, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveOccupancyTemplateView> ToAdaptiveOccupancyTemplateViews(List<AdaptiveOccupancyTemplate> items)
    {
        var views = new List<AdaptiveOccupancyTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveOccupancyTemplateView(item));
        }
        return views;
    }

    public static double SumAdaptiveOccupancyTemplateView(AdaptiveOccupancyTemplateView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
