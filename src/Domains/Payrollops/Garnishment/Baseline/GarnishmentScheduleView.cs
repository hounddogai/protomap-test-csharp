namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment schedule.</summary>
public sealed record GarnishmentScheduleView(string Code, string TitleText, string Capacity, string StartsAt, string Grade, string Maximum, string ArchivedText, string TenureMonthsText, string Team, string Currency, string Cost, string StatusLabel)
{
    public static GarnishmentScheduleView ToGarnishmentScheduleView(GarnishmentSchedule item)
    {
        return new GarnishmentScheduleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            Text.Truncate(item.Currency, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentScheduleView> ToGarnishmentScheduleViews(List<GarnishmentSchedule> items)
    {
        var views = new List<GarnishmentScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentScheduleView(item));
        }
        return views;
    }

    public static double SumGarnishmentScheduleView(GarnishmentScheduleView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedStartsAt + parsedMaximum;
    }
}
