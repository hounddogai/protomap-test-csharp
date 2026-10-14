namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition review.</summary>
public sealed record RecognitionReviewView(string CodeText, string Title, string Region, string Limit, string OwnerId, string Channel, string StatusLabel)
{
    public static RecognitionReviewView ToRecognitionReviewView(RecognitionReview item)
    {
        return new RecognitionReviewView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Region,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionReviewView> ToRecognitionReviewViews(List<RecognitionReview> items)
    {
        var views = new List<RecognitionReviewView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionReviewView(item));
        }
        return views;
    }

    public static double SumRecognitionReviewView(RecognitionReviewView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
