namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday override.</summary>
public sealed record HolidayOverrideView(string Code, string TitleText, string SummaryText, string Checksum, string ChannelText, string Approved, string PhoneText, string Score, string StatusLabel)
{
    public static HolidayOverrideView ToHolidayOverrideView(HolidayOverride item)
    {
        return new HolidayOverrideView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Summary, 40),
            Text.Truncate(item.Checksum, 120),
            item.Channel,
            $"{item.Approved}",
            item.Phone,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayOverrideView> ToHolidayOverrideViews(List<HolidayOverride> items)
    {
        var views = new List<HolidayOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayOverrideView(item));
        }
        return views;
    }

    public static double SumHolidayOverrideView(HolidayOverrideView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
