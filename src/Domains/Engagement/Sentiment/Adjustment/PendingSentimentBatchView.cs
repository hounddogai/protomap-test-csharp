namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending sentiment batch.</summary>
public sealed record PendingSentimentBatchView(string Code, string Name, string Archived, string OwnerId, string Quantity, string Reviewer, string StatusLabel)
{
    public static PendingSentimentBatchView ToPendingSentimentBatchView(PendingSentimentBatch item)
    {
        return new PendingSentimentBatchView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Archived}",
            item.OwnerId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingSentimentBatchView> ToPendingSentimentBatchViews(List<PendingSentimentBatch> items)
    {
        var views = new List<PendingSentimentBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPendingSentimentBatchView(item));
        }
        return views;
    }

    public static double SumPendingSentimentBatchView(PendingSentimentBatchView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
