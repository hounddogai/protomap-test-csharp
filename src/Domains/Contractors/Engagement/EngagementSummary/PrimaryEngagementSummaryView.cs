namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary engagement summary.</summary>
public sealed record PrimaryEngagementSummaryView(string CodeText, string Title, string WindowDaysText, string GraceDays, string Score, string StatusLabel)
{
    public static PrimaryEngagementSummaryView ToPrimaryEngagementSummaryView(PrimaryEngagementSummary item)
    {
        return new PrimaryEngagementSummaryView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryEngagementSummaryView> ToPrimaryEngagementSummaryViews(List<PrimaryEngagementSummary> items)
    {
        var views = new List<PrimaryEngagementSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryEngagementSummaryView(item));
        }
        return views;
    }

    public static double SumPrimaryEngagementSummaryView(PrimaryEngagementSummaryView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedGraceDays + parsedScore;
    }
}
