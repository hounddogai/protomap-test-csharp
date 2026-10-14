namespace Acme.Workforce.Absence.Holiday.Variance;

using System.Globalization;

/// <summary>How screens and exports show a adaptive holiday variance.</summary>
public sealed record AdaptiveHolidayVarianceView(string CodeText, string Label, string WindowDaysText, string Retries, string Margin, string PinnedText, string Amount, string StatusLabel)
{
    public static AdaptiveHolidayVarianceView ToAdaptiveHolidayVarianceView(AdaptiveHolidayVariance item)
    {
        return new AdaptiveHolidayVarianceView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveHolidayVarianceView> ToAdaptiveHolidayVarianceViews(List<AdaptiveHolidayVariance> items)
    {
        var views = new List<AdaptiveHolidayVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveHolidayVarianceView(item));
        }
        return views;
    }

    public static double SumAdaptiveHolidayVarianceView(AdaptiveHolidayVarianceView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedRetries + parsedMargin;
    }
}
