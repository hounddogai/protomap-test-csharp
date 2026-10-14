namespace Acme.Workforce.Absence.Holiday.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a holiday notice.</summary>
public sealed record HolidayNoticeView(string CodeText, string Name, string CostText, string Skill, string Region, string SummaryText, string Locale, string StatusLabel)
{
    public static HolidayNoticeView ToHolidayNoticeView(HolidayNotice item)
    {
        return new HolidayNoticeView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Region,
            item.Summary,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayNoticeView> ToHolidayNoticeViews(List<HolidayNotice> items)
    {
        var views = new List<HolidayNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayNoticeView(item));
        }
        return views;
    }

    public static double SumHolidayNoticeView(HolidayNoticeView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
