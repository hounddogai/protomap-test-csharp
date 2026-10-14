namespace Acme.Workforce.Talent.Mobility.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a mobility segment.</summary>
public sealed record MobilitySegmentView(string Code, string Name, string Amount, string Minimum, string Retries, string WindowDays, string TitleText, string LimitText, string StatusLabel)
{
    public static MobilitySegmentView ToMobilitySegmentView(MobilitySegment item)
    {
        return new MobilitySegmentView(
            item.Code,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilitySegmentView> ToMobilitySegmentViews(List<MobilitySegment> items)
    {
        var views = new List<MobilitySegmentView>();
        foreach (var item in items)
        {
            views.Add(ToMobilitySegmentView(item));
        }
        return views;
    }

    public static double SumMobilitySegmentView(MobilitySegmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMinimum + (double)parsedRetries;
    }
}
