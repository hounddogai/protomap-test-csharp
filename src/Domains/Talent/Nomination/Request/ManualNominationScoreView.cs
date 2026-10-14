namespace Acme.Workforce.Talent.Nomination.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual nomination score.</summary>
public sealed record ManualNominationScoreView(string Code, string Title, string ManagerIdText, string MaximumText, string ScoreText, string Summary, string Ceiling, string Rating, string Country, string StatusLabel)
{
    public static ManualNominationScoreView ToManualNominationScoreView(ManualNominationScore item)
    {
        return new ManualNominationScoreView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.ManagerId,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualNominationScoreView> ToManualNominationScoreViews(List<ManualNominationScore> items)
    {
        var views = new List<ManualNominationScoreView>();
        foreach (var item in items)
        {
            views.Add(ToManualNominationScoreView(item));
        }
        return views;
    }

    public static double SumManualNominationScoreView(ManualNominationScoreView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedScore + parsedCeiling;
    }
}
