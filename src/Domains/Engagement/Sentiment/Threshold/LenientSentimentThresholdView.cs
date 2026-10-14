namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient sentiment threshold.</summary>
public sealed record LenientSentimentThresholdView(string Code, string Label, string SalaryBand, string Country, string GraceDays, string Amount, string FloorValue, string Category, string DurationMinutesText, string TimezoneText, string StatusLabel)
{
    public static LenientSentimentThresholdView ToLenientSentimentThresholdView(LenientSentimentThreshold item)
    {
        return new LenientSentimentThresholdView(
            item.Code,
            item.Label,
            Text.Truncate(item.SalaryBand, 40),
            item.Country,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSentimentThresholdView> ToLenientSentimentThresholdViews(List<LenientSentimentThreshold> items)
    {
        var views = new List<LenientSentimentThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSentimentThresholdView(item));
        }
        return views;
    }

    public static double SumLenientSentimentThresholdView(LenientSentimentThresholdView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedAmount + parsedFloorValue;
    }
}
