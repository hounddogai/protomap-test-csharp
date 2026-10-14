namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Functions of audit cycle from ledger.</summary>
public static class AuditCycleFromLedger
{
    public static Dictionary<string, double> SummarizeAttestationLedgersForCycle(List<AttestationLedger> items)
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

    public static List<AuditCycle> DeriveAuditCyclesFromLedger(List<AttestationLedger> items)
    {
        var outValue = new List<AuditCycle>();
        foreach (var item in items)
        {
            var derived = AuditCycle.NewAuditCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanAuditCycle(AuditCycle item)
    {
        return CappedAttestationProposalPolicy.ClassifyCappedAttestationProposal(item.AdjustedTotal());
    }
}
