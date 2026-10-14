namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using System.Globalization;

/// <summary>How screens and exports show a fallback pulse survey segment.</summary>
public sealed record FallbackPulseSurveySegmentView(string Code, string Name, string Grade, string DurationMinutes, string RatingText, string StatusLabel)
{
    public static FallbackPulseSurveySegmentView ToFallbackPulseSurveySegmentView(FallbackPulseSurveySegment item)
    {
        return new FallbackPulseSurveySegmentView(
            item.Code,
            item.Name,
            item.Grade,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPulseSurveySegmentView> ToFallbackPulseSurveySegmentViews(List<FallbackPulseSurveySegment> items)
    {
        var views = new List<FallbackPulseSurveySegmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPulseSurveySegmentView(item));
        }
        return views;
    }

    public static double SumFallbackPulseSurveySegmentView(FallbackPulseSurveySegmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRating;
    }
}
