namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run schedule.</summary>
public sealed record PayRunScheduleView(string Code, string Name, string RatingText, string Locale, string Hours, string Badge, string Threshold, string ExpiresAt, string StatusLabel)
{
    public static PayRunScheduleView ToPayRunScheduleView(PayRunSchedule item)
    {
        return new PayRunScheduleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunScheduleView> ToPayRunScheduleViews(List<PayRunSchedule> items)
    {
        var views = new List<PayRunScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunScheduleView(item));
        }
        return views;
    }

    public static double SumPayRunScheduleView(PayRunScheduleView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedRating + parsedHours + parsedThreshold;
    }
}
