namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Functions of attestation tier from plan.</summary>
public static class AttestationTierFromPlan
{
    public static Dictionary<string, double> SummarizePendingExceptionPlansForTier(List<PendingExceptionPlan> items)
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

    public static List<AttestationTier> DeriveAttestationTiersFromPlan(List<PendingExceptionPlan> items)
    {
        var outValue = new List<AttestationTier>();
        foreach (var item in items)
        {
            var derived = AttestationTier.NewAttestationTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyAttestationTier(AttestationTier item)
    {
        return PendingExceptionPlanPolicy.PendingExceptionPlanWeight(6, true);
    }
}
