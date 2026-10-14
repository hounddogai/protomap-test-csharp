namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using System.Globalization;

/// <summary>How screens and exports show a seasonal merit cycle rollup.</summary>
public sealed record SeasonalMeritCycleRollupView(string Code, string Title, string Weight, string GradeText, string Ceiling, string EndsAt, string Ratio, string Priority, string StatusLabel)
{
    public static SeasonalMeritCycleRollupView ToSeasonalMeritCycleRollupView(SeasonalMeritCycleRollup item)
    {
        return new SeasonalMeritCycleRollupView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalMeritCycleRollupView> ToSeasonalMeritCycleRollupViews(List<SeasonalMeritCycleRollup> items)
    {
        var views = new List<SeasonalMeritCycleRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalMeritCycleRollupView(item));
        }
        return views;
    }

    public static double SumSeasonalMeritCycleRollupView(SeasonalMeritCycleRollupView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCeiling + (double)parsedEndsAt;
    }
}
