namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered scenario policy.</summary>
public sealed record TieredScenarioPolicyView(string Code, string Title, string RateText, string StartsAt, string Pinned, string Cost, string StatusLabel)
{
    public static TieredScenarioPolicyView ToTieredScenarioPolicyView(TieredScenarioPolicy item)
    {
        return new TieredScenarioPolicyView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredScenarioPolicyView> ToTieredScenarioPolicyViews(List<TieredScenarioPolicy> items)
    {
        var views = new List<TieredScenarioPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToTieredScenarioPolicyView(item));
        }
        return views;
    }

    public static double SumTieredScenarioPolicyView(TieredScenarioPolicyView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedStartsAt + parsedCost;
    }
}
