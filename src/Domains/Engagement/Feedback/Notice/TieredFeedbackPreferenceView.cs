namespace Acme.Workforce.Engagement.Feedback.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered feedback preference.</summary>
public sealed record TieredFeedbackPreferenceView(string Code, string NameText, string OwnerId, string Rating, string TimezoneText, string Locale, string Retries, string Archived, string DurationMinutes, string CategoryText, string StatusLabel)
{
    public static TieredFeedbackPreferenceView ToTieredFeedbackPreferenceView(TieredFeedbackPreference item)
    {
        return new TieredFeedbackPreferenceView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.OwnerId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            item.Locale,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredFeedbackPreferenceView> ToTieredFeedbackPreferenceViews(List<TieredFeedbackPreference> items)
    {
        var views = new List<TieredFeedbackPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToTieredFeedbackPreferenceView(item));
        }
        return views;
    }

    public static double SumTieredFeedbackPreferenceView(TieredFeedbackPreferenceView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedRetries + (double)parsedDurationMinutes;
    }
}
