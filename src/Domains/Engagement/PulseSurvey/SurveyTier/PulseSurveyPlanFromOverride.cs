namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Functions of pulse survey plan from override.</summary>
public static class PulseSurveyPlanFromOverride
{
    public static Dictionary<string, double> SummarizeFallbackRecognitionOverridesForPlan(List<FallbackRecognitionOverride> items)
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

    public static List<PulseSurveyPlan> DerivePulseSurveyPlansFromOverride(List<FallbackRecognitionOverride> items)
    {
        var outValue = new List<PulseSurveyPlan>();
        foreach (var item in items)
        {
            var derived = PulseSurveyPlan.NewPulseSurveyPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePulseSurveyPlan(PulseSurveyPlan item)
    {
        return RecognitionProposalPolicy.ClassifyRecognitionProposal(item.AdjustedValue());
    }
}
