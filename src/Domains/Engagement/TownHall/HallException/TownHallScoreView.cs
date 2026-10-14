namespace Acme.Workforce.Engagement.TownHall.HallException;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall score.</summary>
public sealed record TownHallScoreView(string Code, string Name, string ContactEmailText, string Currency, string Title, string Amount, string StatusLabel)
{
    public static TownHallScoreView ToTownHallScoreView(TownHallScore item)
    {
        return new TownHallScoreView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.ContactEmail,
            Text.Truncate(item.Currency, 80),
            Text.Truncate(item.Title, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallScoreView> ToTownHallScoreViews(List<TownHallScore> items)
    {
        var views = new List<TownHallScoreView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallScoreView(item));
        }
        return views;
    }

    public static double SumTownHallScoreView(TownHallScoreView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
