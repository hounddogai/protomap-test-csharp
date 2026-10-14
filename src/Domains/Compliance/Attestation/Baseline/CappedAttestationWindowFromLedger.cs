namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>Functions of capped attestation window from ledger.</summary>
public static class CappedAttestationWindowFromLedger
{
    public static Dictionary<string, double> SummarizeWeightedAuditLedgersForWindow(List<WeightedAuditLedger> items)
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

    public static List<CappedAttestationWindow> DeriveCappedAttestationWindowsFromLedger(List<WeightedAuditLedger> items)
    {
        var outValue = new List<CappedAttestationWindow>();
        foreach (var item in items)
        {
            var derived = CappedAttestationWindow.NewCappedAttestationWindow(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCappedAttestationWindow(CappedAttestationWindow item)
    {
        return AuditCyclePolicy.NormalizeAuditCycleCode(item.Code);
    }
}
