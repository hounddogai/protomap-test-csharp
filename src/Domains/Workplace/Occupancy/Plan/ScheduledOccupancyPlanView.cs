namespace Acme.Workforce.Workplace.Occupancy.Plan;

using System.Globalization;

/// <summary>How screens and exports show a scheduled occupancy plan.</summary>
public sealed record ScheduledOccupancyPlanView(string Code, string Name, string Locale, string StartsAtText, string ExpiresAt, string StatusLabel)
{
    public static ScheduledOccupancyPlanView ToScheduledOccupancyPlanView(ScheduledOccupancyPlan item)
    {
        return new ScheduledOccupancyPlanView(
            item.Code,
            item.Name,
            item.Locale,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledOccupancyPlanView> ToScheduledOccupancyPlanViews(List<ScheduledOccupancyPlan> items)
    {
        var views = new List<ScheduledOccupancyPlanView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledOccupancyPlanView(item));
        }
        return views;
    }

    public static double SumScheduledOccupancyPlanView(ScheduledOccupancyPlanView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedExpiresAt;
    }
}
