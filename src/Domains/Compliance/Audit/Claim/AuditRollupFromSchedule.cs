namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Functions of audit rollup from schedule.</summary>
public static class AuditRollupFromSchedule
{
    public static Dictionary<string, double> SummarizeAttestationSchedulesForRollup(List<AttestationSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AuditRollup> DeriveAuditRollupsFromSchedule(List<AttestationSchedule> items)
    {
        var outValue = new List<AuditRollup>();
        foreach (var item in items)
        {
            var derived = AuditRollup.NewAuditRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileAuditRollup(AuditRollup item)
    {
        return DefaultAttestationClaimPolicy.DefaultAttestationClaimWeight(4, true);
    }
}
