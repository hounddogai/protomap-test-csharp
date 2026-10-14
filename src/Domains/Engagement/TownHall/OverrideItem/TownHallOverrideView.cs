namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall override.</summary>
public sealed record TownHallOverrideView(string CodeText, string Title, string Rating, string GradeText, string BatchSize, string StatusLabel)
{
    public static TownHallOverrideView ToTownHallOverrideView(TownHallOverride item)
    {
        return new TownHallOverrideView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallOverrideView> ToTownHallOverrideViews(List<TownHallOverride> items)
    {
        var views = new List<TownHallOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallOverrideView(item));
        }
        return views;
    }

    public static double SumTownHallOverrideView(TownHallOverrideView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedBatchSize;
    }
}
