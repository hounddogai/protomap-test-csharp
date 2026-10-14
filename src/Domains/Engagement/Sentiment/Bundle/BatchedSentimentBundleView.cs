namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a batched sentiment bundle.</summary>
public sealed record BatchedSentimentBundleView(string CodeText, string Label, string Amount, string NotesText, string Weight, string TitleText, string StartsAt, string ExternalRefText, string ScoreText, string OwnerIdText, string StatusLabel)
{
    public static BatchedSentimentBundleView ToBatchedSentimentBundleView(BatchedSentimentBundle item)
    {
        return new BatchedSentimentBundleView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedSentimentBundleView> ToBatchedSentimentBundleViews(List<BatchedSentimentBundle> items)
    {
        var views = new List<BatchedSentimentBundleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedSentimentBundleView(item));
        }
        return views;
    }

    public static double SumBatchedSentimentBundleView(BatchedSentimentBundleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedAmount + parsedWeight + (double)parsedStartsAt;
    }
}
