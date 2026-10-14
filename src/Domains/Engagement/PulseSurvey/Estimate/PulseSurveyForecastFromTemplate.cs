namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Functions of pulse survey forecast from template.</summary>
public static class PulseSurveyForecastFromTemplate
{
    public static Dictionary<string, double> SummarizeCappedRecognitionTemplatesForForecast(List<CappedRecognitionTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PulseSurveyForecast> DerivePulseSurveyForecastsFromTemplate(List<CappedRecognitionTemplate> items)
    {
        var outValue = new List<PulseSurveyForecast>();
        foreach (var item in items)
        {
            var derived = PulseSurveyForecast.NewPulseSurveyForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePulseSurveyForecast(PulseSurveyForecast item)
    {
        return CappedRecognitionTemplatePolicy.NormalizeCappedRecognitionTemplateCode(item.Code);
    }
}
