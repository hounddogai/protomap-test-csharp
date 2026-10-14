namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal cost model preference.</summary>
public sealed record SeasonalCostModelPreferenceView(string Code, string NameText, string FloorValue, string CountryText, string Quantity, string Team, string Rate, string StatusLabel)
{
    public static SeasonalCostModelPreferenceView ToSeasonalCostModelPreferenceView(SeasonalCostModelPreference item)
    {
        return new SeasonalCostModelPreferenceView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalCostModelPreferenceView> ToSeasonalCostModelPreferenceViews(List<SeasonalCostModelPreference> items)
    {
        var views = new List<SeasonalCostModelPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalCostModelPreferenceView(item));
        }
        return views;
    }

    public static double SumSeasonalCostModelPreferenceView(SeasonalCostModelPreferenceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedQuantity + parsedRate;
    }
}
