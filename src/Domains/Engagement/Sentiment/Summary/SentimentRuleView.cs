namespace Acme.Workforce.Engagement.Sentiment.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment rule.</summary>
public sealed record SentimentRuleView(string CodeText, string Name, string Channel, string ThresholdText, string ContactEmail, string LeaveDays, string Grade, string Currency, string TenureMonthsText, string JobCode, string StatusLabel)
{
    public static SentimentRuleView ToSentimentRuleView(SentimentRule item)
    {
        return new SentimentRuleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Channel, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            item.Currency,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentRuleView> ToSentimentRuleViews(List<SentimentRule> items)
    {
        var views = new List<SentimentRuleView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentRuleView(item));
        }
        return views;
    }

    public static double SumSentimentRuleView(SentimentRuleView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLeaveDays + (double)parsedTenureMonths;
    }
}
