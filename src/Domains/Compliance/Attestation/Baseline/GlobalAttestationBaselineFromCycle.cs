namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Functions of global attestation baseline from cycle.</summary>
public static class GlobalAttestationBaselineFromCycle
{
    public static Dictionary<string, double> SummarizeLenientPolicyAckCyclesForBaseline(List<LenientPolicyAckCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<GlobalAttestationBaseline> DeriveGlobalAttestationBaselinesFromCycle(List<LenientPolicyAckCycle> items)
    {
        var outValue = new List<GlobalAttestationBaseline>();
        foreach (var item in items)
        {
            var derived = GlobalAttestationBaseline.NewGlobalAttestationBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanGlobalAttestationBaseline(GlobalAttestationBaseline item)
    {
        return LenientPolicyAckCyclePolicy.LenientPolicyAckCycleBucket(item.EffectiveValue());
    }
}
