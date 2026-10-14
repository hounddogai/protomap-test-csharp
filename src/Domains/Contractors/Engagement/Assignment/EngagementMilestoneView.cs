namespace Acme.Workforce.Contractors.Engagement.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement milestone.</summary>
public sealed record EngagementMilestoneView(string Code, string LabelText, string Hours, string DurationMinutesText, string Rate, string StatusLabel)
{
    public static EngagementMilestoneView ToEngagementMilestoneView(EngagementMilestone item)
    {
        return new EngagementMilestoneView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementMilestoneView> ToEngagementMilestoneViews(List<EngagementMilestone> items)
    {
        var views = new List<EngagementMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementMilestoneView(item));
        }
        return views;
    }

    public static double SumEngagementMilestoneView(EngagementMilestoneView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedDurationMinutes + parsedRate;
    }
}
