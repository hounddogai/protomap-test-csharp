namespace Acme.Workforce.Compensation.PayBand.Quota;

using System.Globalization;

/// <summary>How screens and exports show a seasonal pay band checkpoint.</summary>
public sealed record SeasonalPayBandCheckpointView(string Code, string Label, string Margin, string SourceText, string Country, string WindowDaysText, string StatusLabel)
{
    public static SeasonalPayBandCheckpointView ToSeasonalPayBandCheckpointView(SeasonalPayBandCheckpoint item)
    {
        return new SeasonalPayBandCheckpointView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Country,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayBandCheckpointView> ToSeasonalPayBandCheckpointViews(List<SeasonalPayBandCheckpoint> items)
    {
        var views = new List<SeasonalPayBandCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayBandCheckpointView(item));
        }
        return views;
    }

    public static double SumSeasonalPayBandCheckpointView(SeasonalPayBandCheckpointView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedWindowDays;
    }
}
