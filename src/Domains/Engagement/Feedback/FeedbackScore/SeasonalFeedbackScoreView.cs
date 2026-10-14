namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal feedback score.</summary>
public sealed record SeasonalFeedbackScoreView(string Code, string TitleText, string Location, string TimezoneText, string Rating, string TenureMonths, string CountryText, string StatusLabel)
{
    public static SeasonalFeedbackScoreView ToSeasonalFeedbackScoreView(SeasonalFeedbackScore item)
    {
        return new SeasonalFeedbackScoreView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Location,
            Text.Truncate(item.Timezone, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFeedbackScoreView> ToSeasonalFeedbackScoreViews(List<SeasonalFeedbackScore> items)
    {
        var views = new List<SeasonalFeedbackScoreView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFeedbackScoreView(item));
        }
        return views;
    }

    public static double SumSeasonalFeedbackScoreView(SeasonalFeedbackScoreView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedTenureMonths;
    }
}
