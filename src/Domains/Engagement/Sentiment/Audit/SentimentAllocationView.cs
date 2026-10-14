namespace Acme.Workforce.Engagement.Sentiment.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment allocation.</summary>
public sealed record SentimentAllocationView(string Code, string Name, string Approved, string RetriesText, string Rate, string StatusLabel)
{
    public static SentimentAllocationView ToSentimentAllocationView(SentimentAllocation item)
    {
        return new SentimentAllocationView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Approved}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentAllocationView> ToSentimentAllocationViews(List<SentimentAllocation> items)
    {
        var views = new List<SentimentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentAllocationView(item));
        }
        return views;
    }

    public static double SumSentimentAllocationView(SentimentAllocationView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRate;
    }
}
