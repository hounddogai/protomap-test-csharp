namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment assignment.</summary>
public sealed record SentimentAssignmentView(string CodeText, string TitleText, string BatchSize, string TenureMonths, string StatusLabel)
{
    public static SentimentAssignmentView ToSentimentAssignmentView(SentimentAssignment item)
    {
        return new SentimentAssignmentView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentAssignmentView> ToSentimentAssignmentViews(List<SentimentAssignment> items)
    {
        var views = new List<SentimentAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentAssignmentView(item));
        }
        return views;
    }

    public static double SumSentimentAssignmentView(SentimentAssignmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedTenureMonths;
    }
}
