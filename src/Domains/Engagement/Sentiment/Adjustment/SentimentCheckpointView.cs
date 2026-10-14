namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment checkpoint.</summary>
public sealed record SentimentCheckpointView(string CodeText, string Name, string UnitText, string GraceDaysText, string PhoneText, string Fte, string StatusLabel)
{
    public static SentimentCheckpointView ToSentimentCheckpointView(SentimentCheckpoint item)
    {
        return new SentimentCheckpointView(
            item.Code,
            item.Name,
            Text.Truncate(item.Unit, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentCheckpointView> ToSentimentCheckpointViews(List<SentimentCheckpoint> items)
    {
        var views = new List<SentimentCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentCheckpointView(item));
        }
        return views;
    }

    public static double SumSentimentCheckpointView(SentimentCheckpointView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFte;
    }
}
