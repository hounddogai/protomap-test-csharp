namespace Acme.Workforce.Workplace.Desk.Tier;

using System.Globalization;

/// <summary>How screens and exports show a regional desk summary.</summary>
public sealed record RegionalDeskSummaryView(string CodeText, string Title, string Phone, string Active, string WindowDays, string Timezone, string StatusLabel)
{
    public static RegionalDeskSummaryView ToRegionalDeskSummaryView(RegionalDeskSummary item)
    {
        return new RegionalDeskSummaryView(
            item.Code,
            item.Title,
            item.Phone,
            $"{item.Active}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeskSummaryView> ToRegionalDeskSummaryViews(List<RegionalDeskSummary> items)
    {
        var views = new List<RegionalDeskSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeskSummaryView(item));
        }
        return views;
    }

    public static double SumRegionalDeskSummaryView(RegionalDeskSummaryView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
