namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a lenient visitor schedule.</summary>
public sealed record LenientVisitorScheduleView(string Code, string Label, string GraceDaysText, string Maximum, string Limit, string Budget, string StatusLabel)
{
    public static LenientVisitorScheduleView ToLenientVisitorScheduleView(LenientVisitorSchedule item)
    {
        return new LenientVisitorScheduleView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientVisitorScheduleView> ToLenientVisitorScheduleViews(List<LenientVisitorSchedule> items)
    {
        var views = new List<LenientVisitorScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientVisitorScheduleView(item));
        }
        return views;
    }

    public static double SumLenientVisitorScheduleView(LenientVisitorScheduleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedMaximum + (double)parsedLimit;
    }
}
