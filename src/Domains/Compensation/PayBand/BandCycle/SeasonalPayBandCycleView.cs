namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using System.Globalization;

/// <summary>How screens and exports show a seasonal pay band cycle.</summary>
public sealed record SeasonalPayBandCycleView(string Code, string Label, string Margin, string GraceDays, string WindowDays, string Checksum, string SegmentText, string CapacityText, string DurationMinutes, string StatusLabel)
{
    public static SeasonalPayBandCycleView ToSeasonalPayBandCycleView(SeasonalPayBandCycle item)
    {
        return new SeasonalPayBandCycleView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Segment,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayBandCycleView> ToSeasonalPayBandCycleViews(List<SeasonalPayBandCycle> items)
    {
        var views = new List<SeasonalPayBandCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayBandCycleView(item));
        }
        return views;
    }

    public static double SumSeasonalPayBandCycleView(SeasonalPayBandCycleView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedGraceDays + (double)parsedWindowDays;
    }
}
