namespace Acme.Workforce.Engagement.TownHall.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall summary.</summary>
public sealed record TownHallSummaryView(string CodeText, string Label, string ExpiresAt, string GraceDays, string Currency, string StatusLabel)
{
    public static TownHallSummaryView ToTownHallSummaryView(TownHallSummary item)
    {
        return new TownHallSummaryView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallSummaryView> ToTownHallSummaryViews(List<TownHallSummary> items)
    {
        var views = new List<TownHallSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallSummaryView(item));
        }
        return views;
    }

    public static double SumTownHallSummaryView(TownHallSummaryView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays;
    }
}
