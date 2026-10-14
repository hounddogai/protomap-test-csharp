namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>Functions of capped pulse survey profile from agreement.</summary>
public static class CappedPulseSurveyProfileFromAgreement
{
    public static Dictionary<string, double> SummarizeRecognitionAgreementsForProfile(List<RecognitionAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CappedPulseSurveyProfile> DeriveCappedPulseSurveyProfilesFromAgreement(List<RecognitionAgreement> items)
    {
        var outValue = new List<CappedPulseSurveyProfile>();
        foreach (var item in items)
        {
            var derived = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeCappedPulseSurveyProfile(CappedPulseSurveyProfile item)
    {
        return DefaultRecognitionThresholdPolicy.DefaultRecognitionThresholdWeight(6, true);
    }
}
