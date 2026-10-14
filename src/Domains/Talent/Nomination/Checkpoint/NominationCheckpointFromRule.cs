namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Functions of nomination checkpoint from rule.</summary>
public static class NominationCheckpointFromRule
{
    public static Dictionary<string, double> SummarizePoolRulesForCheckpoint(List<PoolRule> items)
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

    public static List<NominationCheckpoint> DeriveNominationCheckpointsFromRule(List<PoolRule> items)
    {
        var outValue = new List<NominationCheckpoint>();
        foreach (var item in items)
        {
            var derived = NominationCheckpoint.NewNominationCheckpoint(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitNominationCheckpoint(NominationCheckpoint item)
    {
        return PoolAuditPolicy.ClassifyPoolAudit(item.MeasuredValue());
    }
}
