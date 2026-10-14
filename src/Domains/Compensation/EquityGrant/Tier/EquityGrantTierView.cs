namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant tier.</summary>
public sealed record EquityGrantTierView(string Code, string LabelText, string LocationText, string StartsAtText, string Channel, string BudgetText, string StatusLabel)
{
    public static EquityGrantTierView ToEquityGrantTierView(EquityGrantTier item)
    {
        return new EquityGrantTierView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Location, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantTierView> ToEquityGrantTierViews(List<EquityGrantTier> items)
    {
        var views = new List<EquityGrantTierView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantTierView(item));
        }
        return views;
    }

    public static double SumEquityGrantTierView(EquityGrantTierView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedBudget;
    }
}
