namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled feedback forecast.</summary>
public sealed record ScheduledFeedbackForecastView(string Code, string Title, string SalaryBandText, string Cost, string LocaleText, string CurrencyText, string Archived, string EndsAt, string EmployeeIdText, string PinnedText, string StatusLabel)
{
    public static ScheduledFeedbackForecastView ToScheduledFeedbackForecastView(ScheduledFeedbackForecast item)
    {
        return new ScheduledFeedbackForecastView(
            item.Code,
            item.Title,
            item.SalaryBand,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 40),
            item.Currency,
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFeedbackForecastView> ToScheduledFeedbackForecastViews(List<ScheduledFeedbackForecast> items)
    {
        var views = new List<ScheduledFeedbackForecastView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFeedbackForecastView(item));
        }
        return views;
    }

    public static double SumScheduledFeedbackForecastView(ScheduledFeedbackForecastView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedEndsAt;
    }
}
