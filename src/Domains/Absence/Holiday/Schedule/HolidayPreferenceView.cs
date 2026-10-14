namespace Acme.Workforce.Absence.Holiday.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a holiday preference.</summary>
public sealed record HolidayPreferenceView(string Code, string TitleText, string MinimumText, string Phone, string Capacity, string Badge, string StatusLabel)
{
    public static HolidayPreferenceView ToHolidayPreferenceView(HolidayPreference item)
    {
        return new HolidayPreferenceView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayPreferenceView> ToHolidayPreferenceViews(List<HolidayPreference> items)
    {
        var views = new List<HolidayPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayPreferenceView(item));
        }
        return views;
    }

    public static double SumHolidayPreferenceView(HolidayPreferenceView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCapacity;
    }
}
