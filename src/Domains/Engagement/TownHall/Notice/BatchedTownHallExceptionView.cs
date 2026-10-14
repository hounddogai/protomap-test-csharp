namespace Acme.Workforce.Engagement.TownHall.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched town hall exception.</summary>
public sealed record BatchedTownHallExceptionView(string CodeText, string Name, string Country, string Cost, string Unit, string Score, string LocaleText, string Rate, string Active, string Rating, string Title, string StatusLabel)
{
    public static BatchedTownHallExceptionView ToBatchedTownHallExceptionView(BatchedTownHallException item)
    {
        return new BatchedTownHallExceptionView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Country, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTownHallExceptionView> ToBatchedTownHallExceptionViews(List<BatchedTownHallException> items)
    {
        var views = new List<BatchedTownHallExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTownHallExceptionView(item));
        }
        return views;
    }

    public static double SumBatchedTownHallExceptionView(BatchedTownHallExceptionView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedCost + parsedScore + parsedRate;
    }
}
