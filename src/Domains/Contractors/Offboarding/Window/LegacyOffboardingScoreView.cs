namespace Acme.Workforce.Contractors.Offboarding.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy offboarding score.</summary>
public sealed record LegacyOffboardingScoreView(string CodeText, string Label, string Country, string Reviewer, string Badge, string ScoreText, string StatusLabel)
{
    public static LegacyOffboardingScoreView ToLegacyOffboardingScoreView(LegacyOffboardingScore item)
    {
        return new LegacyOffboardingScoreView(
            item.Code,
            item.Label,
            item.Country,
            item.Reviewer,
            Text.Truncate(item.Badge, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyOffboardingScoreView> ToLegacyOffboardingScoreViews(List<LegacyOffboardingScore> items)
    {
        var views = new List<LegacyOffboardingScoreView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyOffboardingScoreView(item));
        }
        return views;
    }

    public static double SumLegacyOffboardingScoreView(LegacyOffboardingScoreView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
