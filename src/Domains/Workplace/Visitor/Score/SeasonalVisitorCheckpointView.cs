namespace Acme.Workforce.Workplace.Visitor.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal visitor checkpoint.</summary>
public sealed record SeasonalVisitorCheckpointView(string Code, string Title, string Pinned, string ManagerId, string WindowDays, string Locale, string StatusLabel)
{
    public static SeasonalVisitorCheckpointView ToSeasonalVisitorCheckpointView(SeasonalVisitorCheckpoint item)
    {
        return new SeasonalVisitorCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Title,
            $"{item.Pinned}",
            item.ManagerId,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalVisitorCheckpointView> ToSeasonalVisitorCheckpointViews(List<SeasonalVisitorCheckpoint> items)
    {
        var views = new List<SeasonalVisitorCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalVisitorCheckpointView(item));
        }
        return views;
    }

    public static double SumSeasonalVisitorCheckpointView(SeasonalVisitorCheckpointView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
