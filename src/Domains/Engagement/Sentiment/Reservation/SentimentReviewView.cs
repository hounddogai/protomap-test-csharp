namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment review.</summary>
public sealed record SentimentReviewView(string CodeText, string Name, string LimitText, string CurrencyText, string LocaleText, string StatusLabel)
{
    public static SentimentReviewView ToSentimentReviewView(SentimentReview item)
    {
        return new SentimentReviewView(
            item.Code,
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            Text.Truncate(item.Locale, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentReviewView> ToSentimentReviewViews(List<SentimentReview> items)
    {
        var views = new List<SentimentReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentReviewView(item));
        }
        return views;
    }

    public static double SumSentimentReviewView(SentimentReviewView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
