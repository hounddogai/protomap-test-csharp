namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling merit cycle target.</summary>
public sealed record RollingMeritCycleTargetView(string Code, string TitleText, string Retries, string Approved, string ExpiresAt, string LeaveDaysText, string Discount, string GradeText, string EndsAtText, string Priority, string StatusLabel)
{
    public static RollingMeritCycleTargetView ToRollingMeritCycleTargetView(RollingMeritCycleTarget item)
    {
        return new RollingMeritCycleTargetView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingMeritCycleTargetView> ToRollingMeritCycleTargetViews(List<RollingMeritCycleTarget> items)
    {
        var views = new List<RollingMeritCycleTargetView>();
        foreach (var item in items)
        {
            views.Add(ToRollingMeritCycleTargetView(item));
        }
        return views;
    }

    public static double SumRollingMeritCycleTargetView(RollingMeritCycleTargetView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedExpiresAt + (double)parsedLeaveDays;
    }
}
