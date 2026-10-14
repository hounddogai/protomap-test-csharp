namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Functions of refresh ledger from audit.</summary>
public static class RefreshLedgerFromAudit
{
    public static Dictionary<string, double> SummarizeLenientLicenseSeatAuditsForLedger(List<LenientLicenseSeatAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<RefreshLedger> DeriveRefreshLedgersFromAudit(List<LenientLicenseSeatAudit> items)
    {
        var outValue = new List<RefreshLedger>();
        foreach (var item in items)
        {
            var derived = RefreshLedger.NewRefreshLedger(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateRefreshLedger(RefreshLedger item)
    {
        return LenientLicenseSeatAuditPolicy.LenientLicenseSeatAuditWeight(4, true);
    }
}
