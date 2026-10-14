namespace Acme.Workforce.Contractors.Engagement.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal engagement rollup.</summary>
public sealed record SeasonalEngagementRollupView(string CodeText, string TitleText, string JobCode, string RetriesText, string Cost, string Department, string GraceDaysText, string TenureMonths, string SequenceText, string StatusLabel)
{
    public static SeasonalEngagementRollupView ToSeasonalEngagementRollupView(SeasonalEngagementRollup item)
    {
        return new SeasonalEngagementRollupView(
            item.Code,
            item.Title,
            Text.Truncate(item.JobCode, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalEngagementRollupView> ToSeasonalEngagementRollupViews(List<SeasonalEngagementRollup> items)
    {
        var views = new List<SeasonalEngagementRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalEngagementRollupView(item));
        }
        return views;
    }

    public static double SumSeasonalEngagementRollupView(SeasonalEngagementRollupView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCost + (double)parsedGraceDays;
    }
}
