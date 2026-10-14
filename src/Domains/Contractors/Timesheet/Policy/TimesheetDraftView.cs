namespace Acme.Workforce.Contractors.Timesheet.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet draft.</summary>
public sealed record TimesheetDraftView(string CodeText, string LabelText, string GraceDays, string ArchivedText, string Active, string MinimumText, string StatusLabel)
{
    public static TimesheetDraftView ToTimesheetDraftView(TimesheetDraft item)
    {
        return new TimesheetDraftView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Active}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetDraftView> ToTimesheetDraftViews(List<TimesheetDraft> items)
    {
        var views = new List<TimesheetDraftView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetDraftView(item));
        }
        return views;
    }

    public static double SumTimesheetDraftView(TimesheetDraftView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedMinimum;
    }
}
