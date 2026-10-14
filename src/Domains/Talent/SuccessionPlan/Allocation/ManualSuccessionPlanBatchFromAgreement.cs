namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Functions of manual succession plan batch from agreement.</summary>
public static class ManualSuccessionPlanBatchFromAgreement
{
    public static Dictionary<string, double> SummarizeCappedPoolAgreementsForBatch(List<CappedPoolAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ManualSuccessionPlanBatch> DeriveManualSuccessionPlanBatchesFromAgreement(List<CappedPoolAgreement> items)
    {
        var outValue = new List<ManualSuccessionPlanBatch>();
        foreach (var item in items)
        {
            var derived = ManualSuccessionPlanBatch.NewManualSuccessionPlanBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyManualSuccessionPlanBatch(ManualSuccessionPlanBatch item)
    {
        return RollingPoolExceptionPolicy.ClassifyRollingPoolException(item.MeasuredValue());
    }
}
