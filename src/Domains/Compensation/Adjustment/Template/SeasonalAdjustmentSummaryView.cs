namespace Acme.Workforce.Compensation.Adjustment.Template;

using System.Globalization;

/// <summary>How screens and exports show a seasonal adjustment summary.</summary>
public sealed record SeasonalAdjustmentSummaryView(string Code, string Title, string Maximum, string VersionText, string Category, string HoursText, string Rate, string Team, string Approved, string StatusLabel)
{
    public static SeasonalAdjustmentSummaryView ToSeasonalAdjustmentSummaryView(SeasonalAdjustmentSummary item)
    {
        return new SeasonalAdjustmentSummaryView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAdjustmentSummaryView> ToSeasonalAdjustmentSummaryViews(List<SeasonalAdjustmentSummary> items)
    {
        var views = new List<SeasonalAdjustmentSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAdjustmentSummaryView(item));
        }
        return views;
    }

    public static double SumSeasonalAdjustmentSummaryView(SeasonalAdjustmentSummaryView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedVersion + parsedHours;
    }
}
