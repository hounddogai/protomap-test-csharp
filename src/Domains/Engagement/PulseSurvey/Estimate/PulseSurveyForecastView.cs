namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey forecast.</summary>
public sealed record PulseSurveyForecastView(string Code, string Title, string Label, string FteText, string Visible, string ChannelText, string Reviewer, string Limit, string SalaryBand, string EndsAt, string StatusLabel)
{
    public static PulseSurveyForecastView ToPulseSurveyForecastView(PulseSurveyForecast item)
    {
        return new PulseSurveyForecastView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Label, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Channel,
            Text.Truncate(item.Reviewer, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyForecastView> ToPulseSurveyForecastViews(List<PulseSurveyForecast> items)
    {
        var views = new List<PulseSurveyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyForecastView(item));
        }
        return views;
    }

    public static double SumPulseSurveyForecastView(PulseSurveyForecastView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLimit + (double)parsedEndsAt;
    }
}
