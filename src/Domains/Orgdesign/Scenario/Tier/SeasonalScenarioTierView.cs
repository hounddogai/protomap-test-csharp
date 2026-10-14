namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal scenario tier.</summary>
public sealed record SeasonalScenarioTierView(string CodeText, string Name, string Amount, string TenureMonths, string Description, string StatusLabel)
{
    public static SeasonalScenarioTierView ToSeasonalScenarioTierView(SeasonalScenarioTier item)
    {
        return new SeasonalScenarioTierView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalScenarioTierView> ToSeasonalScenarioTierViews(List<SeasonalScenarioTier> items)
    {
        var views = new List<SeasonalScenarioTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalScenarioTierView(item));
        }
        return views;
    }

    public static double SumSeasonalScenarioTierView(SeasonalScenarioTierView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedTenureMonths;
    }
}
