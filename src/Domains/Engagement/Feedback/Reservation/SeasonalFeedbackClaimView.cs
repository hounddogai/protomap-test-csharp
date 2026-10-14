namespace Acme.Workforce.Engagement.Feedback.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal feedback claim.</summary>
public sealed record SeasonalFeedbackClaimView(string CodeText, string Title, string EndsAtText, string Maximum, string ReviewerText, string Limit, string Rate, string Cost, string SalaryBandText, string QuantityText, string StatusLabel)
{
    public static SeasonalFeedbackClaimView ToSeasonalFeedbackClaimView(SeasonalFeedbackClaim item)
    {
        return new SeasonalFeedbackClaimView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFeedbackClaimView> ToSeasonalFeedbackClaimViews(List<SeasonalFeedbackClaim> items)
    {
        var views = new List<SeasonalFeedbackClaimView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFeedbackClaimView(item));
        }
        return views;
    }

    public static double SumSeasonalFeedbackClaimView(SeasonalFeedbackClaimView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMaximum + (double)parsedLimit;
    }
}
