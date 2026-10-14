namespace Acme.Workforce.Engagement.Sentiment.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment request.</summary>
public sealed record SentimentRequestView(string Code, string TitleText, string CurrencyText, string Source, string OwnerId, string UpdatedAt, string Score, string BudgetText, string DurationMinutes, string CreatedAtText, string TimezoneText, string StatusLabel)
{
    public static SentimentRequestView ToSentimentRequestView(SentimentRequest item)
    {
        return new SentimentRequestView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Currency,
            item.Source,
            Text.Truncate(item.OwnerId, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentRequestView> ToSentimentRequestViews(List<SentimentRequest> items)
    {
        var views = new List<SentimentRequestView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentRequestView(item));
        }
        return views;
    }

    public static double SumSentimentRequestView(SentimentRequestView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedScore + parsedBudget;
    }
}
