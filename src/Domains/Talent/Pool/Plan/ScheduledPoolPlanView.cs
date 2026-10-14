namespace Acme.Workforce.Talent.Pool.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pool plan.</summary>
public sealed record ScheduledPoolPlanView(string Code, string Name, string Retries, string StartsAt, string Hours, string Currency, string TenureMonths, string Timezone, string EmployeeId, string RateText, string StatusLabel)
{
    public static ScheduledPoolPlanView ToScheduledPoolPlanView(ScheduledPoolPlan item)
    {
        return new ScheduledPoolPlanView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.EmployeeId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPoolPlanView> ToScheduledPoolPlanViews(List<ScheduledPoolPlan> items)
    {
        var views = new List<ScheduledPoolPlanView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPoolPlanView(item));
        }
        return views;
    }

    public static double SumScheduledPoolPlanView(ScheduledPoolPlanView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedStartsAt + parsedHours;
    }
}
