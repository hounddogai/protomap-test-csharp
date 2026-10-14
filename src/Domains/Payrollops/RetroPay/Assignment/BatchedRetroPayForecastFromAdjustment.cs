namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Functions of batched retro pay forecast from adjustment.</summary>
public static class BatchedRetroPayForecastFromAdjustment
{
    public static Dictionary<string, double> SummarizeDeductionAdjustmentsForForecast(List<DeductionAdjustment> items)
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

    public static List<BatchedRetroPayForecast> DeriveBatchedRetroPayForecastsFromAdjustment(List<DeductionAdjustment> items)
    {
        var outValue = new List<BatchedRetroPayForecast>();
        foreach (var item in items)
        {
            var derived = BatchedRetroPayForecast.NewBatchedRetroPayForecast(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyBatchedRetroPayForecast(BatchedRetroPayForecast item)
    {
        return LenientDeductionTierPolicy.LenientDeductionTierWeight(5, true);
    }
}
