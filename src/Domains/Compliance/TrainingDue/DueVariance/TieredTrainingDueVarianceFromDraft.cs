namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Functions of tiered training due variance from draft.</summary>
public static class TieredTrainingDueVarianceFromDraft
{
    public static Dictionary<string, double> SummarizePolicyAckDraftsForVariance(List<PolicyAckDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<TieredTrainingDueVariance> DeriveTieredTrainingDueVariancesFromDraft(List<PolicyAckDraft> items)
    {
        var outValue = new List<TieredTrainingDueVariance>();
        foreach (var item in items)
        {
            var derived = TieredTrainingDueVariance.NewTieredTrainingDueVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredTrainingDueVariance(TieredTrainingDueVariance item)
    {
        return PolicyAckDraftPolicy.ClassifyPolicyAckDraft(item.AdjustedValue());
    }
}
