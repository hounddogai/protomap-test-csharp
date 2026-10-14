namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment reservation.</summary>
public sealed record SentimentReservationView(string Code, string Name, string DiscountText, string Weight, string Reviewer, string Threshold, string Version, string MinimumText, string Team, string StatusLabel)
{
    public static SentimentReservationView ToSentimentReservationView(SentimentReservation item)
    {
        return new SentimentReservationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentReservationView> ToSentimentReservationViews(List<SentimentReservation> items)
    {
        var views = new List<SentimentReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentReservationView(item));
        }
        return views;
    }

    public static double SumSentimentReservationView(SentimentReservationView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedWeight + parsedThreshold;
    }
}
