namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Functions of lenient pay run ledger from cycle.</summary>
public static class LenientPayRunLedgerFromCycle
{
    public static Dictionary<string, double> SummarizeManualTaxFormCyclesForLedger(List<ManualTaxFormCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LenientPayRunLedger> DeriveLenientPayRunLedgersFromCycle(List<ManualTaxFormCycle> items)
    {
        var outValue = new List<LenientPayRunLedger>();
        foreach (var item in items)
        {
            var derived = LenientPayRunLedger.NewLenientPayRunLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitLenientPayRunLedger(LenientPayRunLedger item)
    {
        return TaxFormRequestPolicy.TaxFormRequestWeight(4, true);
    }
}
