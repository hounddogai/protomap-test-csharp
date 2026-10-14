namespace Acme.Workforce.Absence.Holiday.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday window.</summary>
public sealed record HolidayWindowView(string CodeText, string Name, string Minimum, string RatioText, string Rate, string DurationMinutes, string Active, string ExpiresAtText, string Threshold, string StatusLabel)
{
    public static HolidayWindowView ToHolidayWindowView(HolidayWindow item)
    {
        return new HolidayWindowView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayWindowView> ToHolidayWindowViews(List<HolidayWindow> items)
    {
        var views = new List<HolidayWindowView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayWindowView(item));
        }
        return views;
    }

    public static double SumHolidayWindowView(HolidayWindowView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRatio + parsedRate;
    }
}
