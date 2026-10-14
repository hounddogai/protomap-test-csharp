namespace Acme.Workforce.Workplace.Desk.Preference;

using System.Globalization;

/// <summary>How screens and exports show a scheduled desk summary.</summary>
public sealed record ScheduledDeskSummaryView(string Code, string TitleText, string Quantity, string Notes, string Rate, string Reviewer, string ChecksumText, string TenureMonths, string StatusLabel)
{
    public static ScheduledDeskSummaryView ToScheduledDeskSummaryView(ScheduledDeskSummary item)
    {
        return new ScheduledDeskSummaryView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Checksum,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledDeskSummaryView> ToScheduledDeskSummaryViews(List<ScheduledDeskSummary> items)
    {
        var views = new List<ScheduledDeskSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledDeskSummaryView(item));
        }
        return views;
    }

    public static double SumScheduledDeskSummaryView(ScheduledDeskSummaryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedRate + (double)parsedTenureMonths;
    }
}
