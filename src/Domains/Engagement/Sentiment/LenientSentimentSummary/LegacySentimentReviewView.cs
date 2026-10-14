namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy sentiment review.</summary>
public sealed record LegacySentimentReviewView(string CodeText, string Title, string Pinned, string WindowDays, string FloorValueText, string UnitText, string StatusLabel)
{
    public static LegacySentimentReviewView ToLegacySentimentReviewView(LegacySentimentReview item)
    {
        return new LegacySentimentReviewView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            $"{item.Pinned}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySentimentReviewView> ToLegacySentimentReviewViews(List<LegacySentimentReview> items)
    {
        var views = new List<LegacySentimentReviewView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySentimentReviewView(item));
        }
        return views;
    }

    public static double SumLegacySentimentReviewView(LegacySentimentReviewView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue;
    }
}
