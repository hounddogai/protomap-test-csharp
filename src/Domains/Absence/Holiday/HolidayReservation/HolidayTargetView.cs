namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday target.</summary>
public sealed record HolidayTargetView(string Code, string Name, string Retries, string Skill, string LeaveDays, string StatusLabel)
{
    public static HolidayTargetView ToHolidayTargetView(HolidayTarget item)
    {
        return new HolidayTargetView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayTargetView> ToHolidayTargetViews(List<HolidayTarget> items)
    {
        var views = new List<HolidayTargetView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayTargetView(item));
        }
        return views;
    }

    public static double SumHolidayTargetView(HolidayTargetView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedLeaveDays;
    }
}
