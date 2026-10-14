namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Functions of batched tax form assignment from batch.</summary>
public static class BatchedTaxFormAssignmentFromBatch
{
    public static Dictionary<string, double> SummarizeTaxFormBatchesForAssignment(List<TaxFormBatch> items)
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

    public static List<BatchedTaxFormAssignment> DeriveBatchedTaxFormAssignmentsFromBatch(List<TaxFormBatch> items)
    {
        var outValue = new List<BatchedTaxFormAssignment>();
        foreach (var item in items)
        {
            var derived = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreBatchedTaxFormAssignment(BatchedTaxFormAssignment item)
    {
        return TaxFormBatchPolicy.ClassifyTaxFormBatch(item.EffectiveValue());
    }
}
