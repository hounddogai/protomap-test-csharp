namespace Acme.Workforce.Compensation.PayBand.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pay band estimate.</summary>
public sealed record DefaultPayBandEstimateView(string Code, string TitleText, string Reviewer, string Name, string Active, string CostCenter, string DescriptionText, string ExternalRefText, string Notes, string TeamText, string Skill, string ScoreText, string StatusLabel)
{
    public static DefaultPayBandEstimateView ToDefaultPayBandEstimateView(DefaultPayBandEstimate item)
    {
        return new DefaultPayBandEstimateView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Reviewer,
            Text.Truncate(item.Name, 40),
            $"{item.Active}",
            item.CostCenter,
            item.Description,
            Text.Truncate(item.ExternalRef, 64),
            item.Notes,
            item.Team,
            item.Skill,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayBandEstimateView> ToDefaultPayBandEstimateViews(List<DefaultPayBandEstimate> items)
    {
        var views = new List<DefaultPayBandEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayBandEstimateView(item));
        }
        return views;
    }

    public static double SumDefaultPayBandEstimateView(DefaultPayBandEstimateView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
