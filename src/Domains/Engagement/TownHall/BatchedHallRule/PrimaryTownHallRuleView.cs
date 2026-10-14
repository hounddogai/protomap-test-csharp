namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary town hall rule.</summary>
public sealed record PrimaryTownHallRuleView(string Code, string Title, string HoursText, string Rating, string Quantity, string Budget, string MarginText, string Channel, string Summary, string Attempts, string StatusLabel)
{
    public static PrimaryTownHallRuleView ToPrimaryTownHallRuleView(PrimaryTownHallRule item)
    {
        return new PrimaryTownHallRuleView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.Summary,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryTownHallRuleView> ToPrimaryTownHallRuleViews(List<PrimaryTownHallRule> items)
    {
        var views = new List<PrimaryTownHallRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryTownHallRuleView(item));
        }
        return views;
    }

    public static double SumPrimaryTownHallRuleView(PrimaryTownHallRuleView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedHours + parsedRating + (double)parsedQuantity;
    }
}
