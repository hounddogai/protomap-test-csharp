namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a lenient license seat schedule.</summary>
public sealed record LenientLicenseSeatScheduleView(string CodeText, string Label, string AttemptsText, string Priority, string DurationMinutes, string SequenceText, string StatusLabel)
{
    public static LenientLicenseSeatScheduleView ToLenientLicenseSeatScheduleView(LenientLicenseSeatSchedule item)
    {
        return new LenientLicenseSeatScheduleView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLicenseSeatScheduleView> ToLenientLicenseSeatScheduleViews(List<LenientLicenseSeatSchedule> items)
    {
        var views = new List<LenientLicenseSeatScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLicenseSeatScheduleView(item));
        }
        return views;
    }

    public static double SumLenientLicenseSeatScheduleView(LenientLicenseSeatScheduleView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedPriority + (double)parsedDurationMinutes;
    }
}
