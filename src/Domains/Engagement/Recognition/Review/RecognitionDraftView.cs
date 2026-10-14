namespace Acme.Workforce.Engagement.Recognition.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition draft.</summary>
public sealed record RecognitionDraftView(string Code, string Name, string Segment, string UpdatedAtText, string ExpiresAt, string Pinned, string Unit, string Visible, string BatchSize, string Rating, string Timezone, string StatusLabel)
{
    public static RecognitionDraftView ToRecognitionDraftView(RecognitionDraft item)
    {
        return new RecognitionDraftView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Segment,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Unit,
            $"{item.Visible}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionDraftView> ToRecognitionDraftViews(List<RecognitionDraft> items)
    {
        var views = new List<RecognitionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionDraftView(item));
        }
        return views;
    }

    public static double SumRecognitionDraftView(RecognitionDraftView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedExpiresAt + (double)parsedBatchSize;
    }
}
