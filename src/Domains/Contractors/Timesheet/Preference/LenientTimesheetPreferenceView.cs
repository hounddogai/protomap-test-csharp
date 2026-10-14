namespace Acme.Workforce.Contractors.Timesheet.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient timesheet preference.</summary>
public sealed record LenientTimesheetPreferenceView(string Code, string TitleText, string MinimumText, string Grade, string Ceiling, string Country, string Limit, string SequenceText, string RevisionText, string DurationMinutesText, string StatusLabel)
{
    public static LenientTimesheetPreferenceView ToLenientTimesheetPreferenceView(LenientTimesheetPreference item)
    {
        return new LenientTimesheetPreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientTimesheetPreferenceView> ToLenientTimesheetPreferenceViews(List<LenientTimesheetPreference> items)
    {
        var views = new List<LenientTimesheetPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToLenientTimesheetPreferenceView(item));
        }
        return views;
    }

    public static double SumLenientTimesheetPreferenceView(LenientTimesheetPreferenceView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedCeiling + (double)parsedLimit;
    }
}
