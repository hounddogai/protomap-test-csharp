namespace Acme.Workforce.Orgdesign.Position.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive position profile.</summary>
public sealed record AdaptivePositionProfileView(string Code, string Title, string Threshold, string Summary, string PriorityText, string Description, string WindowDaysText, string Unit, string Active, string Label, string StartsAt, string StatusLabel)
{
    public static AdaptivePositionProfileView ToAdaptivePositionProfileView(AdaptivePositionProfile item)
    {
        return new AdaptivePositionProfileView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Active}",
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePositionProfileView> ToAdaptivePositionProfileViews(List<AdaptivePositionProfile> items)
    {
        var views = new List<AdaptivePositionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePositionProfileView(item));
        }
        return views;
    }

    public static double SumAdaptivePositionProfileView(AdaptivePositionProfileView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedPriority + (double)parsedWindowDays;
    }
}
