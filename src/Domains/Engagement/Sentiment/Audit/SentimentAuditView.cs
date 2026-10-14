namespace Acme.Workforce.Engagement.Sentiment.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment audit.</summary>
public sealed record SentimentAuditView(string Code, string LabelText, string Discount, string Department, string Retries, string Weight, string Cost, string ThresholdText, string StatusLabel)
{
    public static SentimentAuditView ToSentimentAuditView(SentimentAudit item)
    {
        return new SentimentAuditView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentAuditView> ToSentimentAuditViews(List<SentimentAudit> items)
    {
        var views = new List<SentimentAuditView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentAuditView(item));
        }
        return views;
    }

    public static double SumSentimentAuditView(SentimentAuditView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedRetries + parsedWeight;
    }
}
