namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Functions of batched garnishment exception from reservation.</summary>
public static class BatchedGarnishmentExceptionFromReservation
{
    public static Dictionary<string, double> SummarizeCappedPayRunReservationsForException(List<CappedPayRunReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<BatchedGarnishmentException> DeriveBatchedGarnishmentExceptionsFromReservation(List<CappedPayRunReservation> items)
    {
        var outValue = new List<BatchedGarnishmentException>();
        foreach (var item in items)
        {
            var derived = BatchedGarnishmentException.NewBatchedGarnishmentException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBatchedGarnishmentException(BatchedGarnishmentException item)
    {
        return LenientPayRunLedgerPolicy.NormalizeLenientPayRunLedgerCode(item.Code);
    }
}
