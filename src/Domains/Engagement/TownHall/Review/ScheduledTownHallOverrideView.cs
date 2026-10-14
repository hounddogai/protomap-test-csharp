namespace Acme.Workforce.Engagement.TownHall.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled town hall override.</summary>
public sealed record ScheduledTownHallOverrideView(string CodeText, string Title, string Checksum, string ScoreText, string NotesText, string Amount, string Rate, string Team, string Reviewer, string StatusLabel)
{
    public static ScheduledTownHallOverrideView ToScheduledTownHallOverrideView(ScheduledTownHallOverride item)
    {
        return new ScheduledTownHallOverrideView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Checksum,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            Text.Truncate(item.Reviewer, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTownHallOverrideView> ToScheduledTownHallOverrideViews(List<ScheduledTownHallOverride> items)
    {
        var views = new List<ScheduledTownHallOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTownHallOverrideView(item));
        }
        return views;
    }

    public static double SumScheduledTownHallOverrideView(ScheduledTownHallOverrideView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedScore + parsedAmount + parsedRate;
    }
}
