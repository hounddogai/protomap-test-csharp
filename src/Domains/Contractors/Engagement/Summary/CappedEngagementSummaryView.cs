namespace Acme.Workforce.Contractors.Engagement.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped engagement summary.</summary>
public sealed record CappedEngagementSummaryView(string Code, string Label, string GradeText, string QuantityText, string Attempts, string Name, string CurrencyText, string ExpiresAt, string Maximum, string ChannelText, string StatusLabel)
{
    public static CappedEngagementSummaryView ToCappedEngagementSummaryView(CappedEngagementSummary item)
    {
        return new CappedEngagementSummaryView(
            item.Code,
            item.Label,
            Text.Truncate(item.Grade, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Currency,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedEngagementSummaryView> ToCappedEngagementSummaryViews(List<CappedEngagementSummary> items)
    {
        var views = new List<CappedEngagementSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToCappedEngagementSummaryView(item));
        }
        return views;
    }

    public static double SumCappedEngagementSummaryView(CappedEngagementSummaryView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedAttempts + (double)parsedExpiresAt;
    }
}
