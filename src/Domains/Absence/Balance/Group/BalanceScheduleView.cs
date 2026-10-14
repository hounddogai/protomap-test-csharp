namespace Acme.Workforce.Absence.Balance.Group;

using System.Globalization;

/// <summary>How screens and exports show a balance schedule.</summary>
public sealed record BalanceScheduleView(string Code, string Title, string FloorValueText, string HoursText, string LeaveDays, string Unit, string StatusLabel)
{
    public static BalanceScheduleView ToBalanceScheduleView(BalanceSchedule item)
    {
        return new BalanceScheduleView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceScheduleView> ToBalanceScheduleViews(List<BalanceSchedule> items)
    {
        var views = new List<BalanceScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceScheduleView(item));
        }
        return views;
    }

    public static double SumBalanceScheduleView(BalanceScheduleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedHours + (double)parsedLeaveDays;
    }
}
