namespace Acme.Workforce.Engagement.TownHall.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall review.</summary>
public sealed record TownHallReviewView(string Code, string Label, string Pinned, string Sequence, string Priority, string Title, string LeaveDays, string Channel, string Version, string Archived, string WeightText, string StatusLabel)
{
    public static TownHallReviewView ToTownHallReviewView(TownHallReview item)
    {
        return new TownHallReviewView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallReviewView> ToTownHallReviewViews(List<TownHallReview> items)
    {
        var views = new List<TownHallReviewView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallReviewView(item));
        }
        return views;
    }

    public static double SumTownHallReviewView(TownHallReviewView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedPriority + (double)parsedLeaveDays;
    }
}
