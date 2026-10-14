namespace Acme.Workforce.Absence.Holiday.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a seasonal holiday rollup.</summary>
public sealed record SeasonalHolidayRollupView(string Code, string LabelText, string Team, string ContactEmailText, string Quantity, string EndsAt, string WeightText, string Cost, string Notes, string StatusLabel)
{
    public static SeasonalHolidayRollupView ToSeasonalHolidayRollupView(SeasonalHolidayRollup item)
    {
        return new SeasonalHolidayRollupView(
            item.Code,
            item.Label,
            item.Team,
            item.ContactEmail,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHolidayRollupView> ToSeasonalHolidayRollupViews(List<SeasonalHolidayRollup> items)
    {
        var views = new List<SeasonalHolidayRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHolidayRollupView(item));
        }
        return views;
    }

    public static double SumSeasonalHolidayRollupView(SeasonalHolidayRollupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedEndsAt + parsedWeight;
    }
}
