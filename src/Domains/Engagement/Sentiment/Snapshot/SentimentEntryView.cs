namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment entry.</summary>
public sealed record SentimentEntryView(string Code, string Name, string CostCenter, string TenureMonths, string Approved, string GraceDays, string StatusLabel)
{
    public static SentimentEntryView ToSentimentEntryView(SentimentEntry item)
    {
        return new SentimentEntryView(
            Text.Truncate(item.Code, 40),
            item.Name,
            Text.Truncate(item.CostCenter, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentEntryView> ToSentimentEntryViews(List<SentimentEntry> items)
    {
        var views = new List<SentimentEntryView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentEntryView(item));
        }
        return views;
    }

    public static double SumSentimentEntryView(SentimentEntryView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedGraceDays;
    }
}
