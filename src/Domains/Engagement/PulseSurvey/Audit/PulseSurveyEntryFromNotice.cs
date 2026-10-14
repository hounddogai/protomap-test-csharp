namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Functions of pulse survey entry from notice.</summary>
public static class PulseSurveyEntryFromNotice
{
    public static Dictionary<string, double> SummarizePulseSurveyNoticesForEntry(List<PulseSurveyNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PulseSurveyEntry> DerivePulseSurveyEntriesFromNotice(List<PulseSurveyNotice> items)
    {
        var outValue = new List<PulseSurveyEntry>();
        foreach (var item in items)
        {
            var derived = PulseSurveyEntry.NewPulseSurveyEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPulseSurveyEntry(PulseSurveyEntry item)
    {
        return PulseSurveyTemplatePolicy.ClassifyPulseSurveyTemplate(item.MeasuredValue());
    }
}
