namespace Acme.Workforce.Engagement.Recognition.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition segment.</summary>
public sealed record RecognitionSegmentView(string Code, string Title, string Pinned, string GradeText, string Minimum, string Segment, string ContactEmailText, string DiscountText, string StatusLabel)
{
    public static RecognitionSegmentView ToRecognitionSegmentView(RecognitionSegment item)
    {
        return new RecognitionSegmentView(
            Text.Truncate(item.Code, 64),
            item.Title,
            $"{item.Pinned}",
            item.Grade,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            Text.Truncate(item.ContactEmail, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionSegmentView> ToRecognitionSegmentViews(List<RecognitionSegment> items)
    {
        var views = new List<RecognitionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionSegmentView(item));
        }
        return views;
    }

    public static double SumRecognitionSegmentView(RecognitionSegmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedDiscount;
    }
}
