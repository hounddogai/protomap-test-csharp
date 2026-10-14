namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered headcount schedule.</summary>
public sealed record TieredHeadcountScheduleView(string Code, string Name, string Channel, string Category, string HoursText, string Timezone, string JobCodeText, string Rate, string TenureMonths, string ExpiresAtText, string StatusLabel)
{
    public static TieredHeadcountScheduleView ToTieredHeadcountScheduleView(TieredHeadcountSchedule item)
    {
        return new TieredHeadcountScheduleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Channel,
            Text.Truncate(item.Category, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            Text.Truncate(item.JobCode, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredHeadcountScheduleView> ToTieredHeadcountScheduleViews(List<TieredHeadcountSchedule> items)
    {
        var views = new List<TieredHeadcountScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredHeadcountScheduleView(item));
        }
        return views;
    }

    public static double SumTieredHeadcountScheduleView(TieredHeadcountScheduleView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedHours + parsedRate + (double)parsedTenureMonths;
    }
}
