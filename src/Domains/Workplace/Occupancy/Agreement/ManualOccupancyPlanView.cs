namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual occupancy plan.</summary>
public sealed record ManualOccupancyPlanView(string Code, string Name, string Segment, string RetriesText, string WeightText, string Skill, string EndsAt, string RatingText, string Attempts, string StatusLabel)
{
    public static ManualOccupancyPlanView ToManualOccupancyPlanView(ManualOccupancyPlan item)
    {
        return new ManualOccupancyPlanView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Segment, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualOccupancyPlanView> ToManualOccupancyPlanViews(List<ManualOccupancyPlan> items)
    {
        var views = new List<ManualOccupancyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToManualOccupancyPlanView(item));
        }
        return views;
    }

    public static double SumManualOccupancyPlanView(ManualOccupancyPlanView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedWeight + (double)parsedEndsAt;
    }
}
