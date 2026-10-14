namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Functions of recognition claim from audit.</summary>
public static class RecognitionClaimFromAudit
{
    public static Dictionary<string, double> SummarizeCappedPulseSurveyAuditsForClaim(List<CappedPulseSurveyAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RecognitionClaim> DeriveRecognitionClaimsFromAudit(List<CappedPulseSurveyAudit> items)
    {
        var outValue = new List<RecognitionClaim>();
        foreach (var item in items)
        {
            var derived = RecognitionClaim.NewRecognitionClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankRecognitionClaim(RecognitionClaim item)
    {
        return GlobalPulseSurveyAdjustmentPolicy.NormalizeGlobalPulseSurveyAdjustmentCode(item.Code);
    }
}
