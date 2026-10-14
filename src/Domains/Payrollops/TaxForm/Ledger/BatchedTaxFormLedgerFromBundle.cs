namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Functions of batched tax form ledger from bundle.</summary>
public static class BatchedTaxFormLedgerFromBundle
{
    public static Dictionary<string, double> SummarizeRetroPayBundlesForLedger(List<RetroPayBundle> items)
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

    public static List<BatchedTaxFormLedger> DeriveBatchedTaxFormLedgersFromBundle(List<RetroPayBundle> items)
    {
        var outValue = new List<BatchedTaxFormLedger>();
        foreach (var item in items)
        {
            var derived = BatchedTaxFormLedger.NewBatchedTaxFormLedger(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceBatchedTaxFormLedger(BatchedTaxFormLedger item)
    {
        return RetroPayBundlePolicy.RetroPayBundleBucket(item.AdjustedTotal());
    }
}
