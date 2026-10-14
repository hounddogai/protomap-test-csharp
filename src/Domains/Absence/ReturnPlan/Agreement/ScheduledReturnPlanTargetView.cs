namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled return plan target.</summary>
public sealed record ScheduledReturnPlanTargetView(string CodeText, string Label, string ChannelText, string Department, string GraceDaysText, string HoursText, string Region, string Currency, string StatusLabel)
{
    public static ScheduledReturnPlanTargetView ToScheduledReturnPlanTargetView(ScheduledReturnPlanTarget item)
    {
        return new ScheduledReturnPlanTargetView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Channel,
            item.Department,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            Text.Truncate(item.Currency, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledReturnPlanTargetView> ToScheduledReturnPlanTargetViews(List<ScheduledReturnPlanTarget> items)
    {
        var views = new List<ScheduledReturnPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledReturnPlanTargetView(item));
        }
        return views;
    }

    public static double SumScheduledReturnPlanTargetView(ScheduledReturnPlanTargetView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedHours;
    }
}
