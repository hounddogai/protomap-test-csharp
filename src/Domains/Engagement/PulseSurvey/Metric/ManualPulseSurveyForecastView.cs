namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pulse survey forecast.</summary>
public sealed record ManualPulseSurveyForecastView(string CodeText, string LabelText, string ExternalRefText, string ManagerId, string Margin, string Limit, string StatusLabel)
{
    public static ManualPulseSurveyForecastView ToManualPulseSurveyForecastView(ManualPulseSurveyForecast item)
    {
        return new ManualPulseSurveyForecastView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.ExternalRef,
            item.ManagerId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPulseSurveyForecastView> ToManualPulseSurveyForecastViews(List<ManualPulseSurveyForecast> items)
    {
        var views = new List<ManualPulseSurveyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToManualPulseSurveyForecastView(item));
        }
        return views;
    }

    public static double SumManualPulseSurveyForecastView(ManualPulseSurveyForecastView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedLimit;
    }
}
