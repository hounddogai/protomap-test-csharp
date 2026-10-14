namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using System.Globalization;

/// <summary>How screens and exports show a fallback access badge estimate.</summary>
public sealed record FallbackAccessBadgeEstimateView(string Code, string Label, string Maximum, string ScoreText, string CountryText, string Team, string StatusLabel)
{
    public static FallbackAccessBadgeEstimateView ToFallbackAccessBadgeEstimateView(FallbackAccessBadgeEstimate item)
    {
        return new FallbackAccessBadgeEstimateView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAccessBadgeEstimateView> ToFallbackAccessBadgeEstimateViews(List<FallbackAccessBadgeEstimate> items)
    {
        var views = new List<FallbackAccessBadgeEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAccessBadgeEstimateView(item));
        }
        return views;
    }

    public static double SumFallbackAccessBadgeEstimateView(FallbackAccessBadgeEstimateView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedScore;
    }
}
