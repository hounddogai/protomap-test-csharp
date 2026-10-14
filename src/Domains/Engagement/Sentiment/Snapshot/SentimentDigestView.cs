namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a sentiment digest.</summary>
public sealed record SentimentDigestView(string Code, string TitleText, string WindowDays, string SalaryBand, string Sequence, string DescriptionText, string StatusLabel)
{
    public static SentimentDigestView ToSentimentDigestView(SentimentDigest item)
    {
        return new SentimentDigestView(
            item.Code,
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentDigestView> ToSentimentDigestViews(List<SentimentDigest> items)
    {
        var views = new List<SentimentDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentDigestView(item));
        }
        return views;
    }

    public static double SumSentimentDigestView(SentimentDigestView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedSequence;
    }
}
