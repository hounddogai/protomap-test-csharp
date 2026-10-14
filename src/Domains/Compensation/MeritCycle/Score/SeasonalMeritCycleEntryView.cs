namespace Acme.Workforce.Compensation.MeritCycle.Score;

using System.Globalization;

/// <summary>How screens and exports show a seasonal merit cycle entry.</summary>
public sealed record SeasonalMeritCycleEntryView(string Code, string Label, string Retries, string Attempts, string StatusLabel)
{
    public static SeasonalMeritCycleEntryView ToSeasonalMeritCycleEntryView(SeasonalMeritCycleEntry item)
    {
        return new SeasonalMeritCycleEntryView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalMeritCycleEntryView> ToSeasonalMeritCycleEntryViews(List<SeasonalMeritCycleEntry> items)
    {
        var views = new List<SeasonalMeritCycleEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalMeritCycleEntryView(item));
        }
        return views;
    }

    public static double SumSeasonalMeritCycleEntryView(SeasonalMeritCycleEntryView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts;
    }
}
