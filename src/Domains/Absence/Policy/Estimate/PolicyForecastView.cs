namespace Acme.Workforce.Absence.Policy.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy forecast.</summary>
public sealed record PolicyForecastView(string Code, string Title, string ScoreText, string Summary, string Currency, string Skill, string Active, string StatusLabel)
{
    public static PolicyForecastView ToPolicyForecastView(PolicyForecast item)
    {
        return new PolicyForecastView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.Currency, 80),
            item.Skill,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyForecastView> ToPolicyForecastViews(List<PolicyForecast> items)
    {
        var views = new List<PolicyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyForecastView(item));
        }
        return views;
    }

    public static double SumPolicyForecastView(PolicyForecastView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
