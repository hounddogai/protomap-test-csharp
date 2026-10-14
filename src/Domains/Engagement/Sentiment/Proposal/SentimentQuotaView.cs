namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment quota.</summary>
public sealed record SentimentQuotaView(string Code, string Name, string UnitText, string Label, string ExpiresAt, string Quantity, string Attempts, string Active, string GraceDays, string StatusLabel)
{
    public static SentimentQuotaView ToSentimentQuotaView(SentimentQuota item)
    {
        return new SentimentQuotaView(
            item.Code,
            item.Name,
            item.Unit,
            Text.Truncate(item.Label, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentQuotaView> ToSentimentQuotaViews(List<SentimentQuota> items)
    {
        var views = new List<SentimentQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentQuotaView(item));
        }
        return views;
    }

    public static double SumSentimentQuotaView(SentimentQuotaView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity + (double)parsedAttempts;
    }
}
