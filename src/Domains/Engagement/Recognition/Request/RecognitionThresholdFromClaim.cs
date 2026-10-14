namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Functions of recognition threshold from claim.</summary>
public static class RecognitionThresholdFromClaim
{
    public static Dictionary<string, double> SummarizePulseSurveyClaimsForThreshold(List<PulseSurveyClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<RecognitionThreshold> DeriveRecognitionThresholdsFromClaim(List<PulseSurveyClaim> items)
    {
        var outValue = new List<RecognitionThreshold>();
        foreach (var item in items)
        {
            var derived = RecognitionThreshold.NewRecognitionThreshold(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanRecognitionThreshold(RecognitionThreshold item)
    {
        return PendingPulseSurveyClaimPolicy.PendingPulseSurveyClaimWeight(4, true);
    }
}
