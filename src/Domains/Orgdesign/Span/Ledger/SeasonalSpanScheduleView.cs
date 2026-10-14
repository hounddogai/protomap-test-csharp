namespace Acme.Workforce.Orgdesign.Span.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal span schedule.</summary>
public sealed record SeasonalSpanScheduleView(string Code, string Name, string Margin, string Approved, string Timezone, string Region, string Source, string Archived, string StatusLabel)
{
    public static SeasonalSpanScheduleView ToSeasonalSpanScheduleView(SeasonalSpanSchedule item)
    {
        return new SeasonalSpanScheduleView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Timezone,
            Text.Truncate(item.Region, 80),
            item.Source,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSpanScheduleView> ToSeasonalSpanScheduleViews(List<SeasonalSpanSchedule> items)
    {
        var views = new List<SeasonalSpanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSpanScheduleView(item));
        }
        return views;
    }

    public static double SumSeasonalSpanScheduleView(SeasonalSpanScheduleView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedMargin;
    }
}
