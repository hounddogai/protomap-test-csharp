namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Functions of tiered pay run ledger from group.</summary>
public static class TieredPayRunLedgerFromGroup
{
    public static Dictionary<string, double> SummarizeGarnishmentGroupsForLedger(List<GarnishmentGroup> items)
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

    public static List<TieredPayRunLedger> DeriveTieredPayRunLedgersFromGroup(List<GarnishmentGroup> items)
    {
        var outValue = new List<TieredPayRunLedger>();
        foreach (var item in items)
        {
            var derived = TieredPayRunLedger.NewTieredPayRunLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredPayRunLedger(TieredPayRunLedger item)
    {
        return LenientGarnishmentBudgetPolicy.NormalizeLenientGarnishmentBudgetCode(item.Code);
    }
}
