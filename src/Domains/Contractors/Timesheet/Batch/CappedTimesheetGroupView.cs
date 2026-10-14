namespace Acme.Workforce.Contractors.Timesheet.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped timesheet group.</summary>
public sealed record CappedTimesheetGroupView(string Code, string Name, string LeaveDays, string LabelText, string Unit, string ChannelText, string EmployeeId, string StatusLabel)
{
    public static CappedTimesheetGroupView ToCappedTimesheetGroupView(CappedTimesheetGroup item)
    {
        return new CappedTimesheetGroupView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Unit,
            Text.Truncate(item.Channel, 80),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedTimesheetGroupView> ToCappedTimesheetGroupViews(List<CappedTimesheetGroup> items)
    {
        var views = new List<CappedTimesheetGroupView>();
        foreach (var item in items)
        {
            views.Add(ToCappedTimesheetGroupView(item));
        }
        return views;
    }

    public static double SumCappedTimesheetGroupView(CappedTimesheetGroupView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
