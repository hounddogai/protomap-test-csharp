namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Functions of adjustment slot from proposal.</summary>
public static class AdjustmentSlotFromProposal
{
    public static Dictionary<string, double> SummarizeBenchmarkProposalsForSlot(List<BenchmarkProposal> items)
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

    public static List<AdjustmentSlot> DeriveAdjustmentSlotsFromProposal(List<BenchmarkProposal> items)
    {
        var outValue = new List<AdjustmentSlot>();
        foreach (var item in items)
        {
            var derived = AdjustmentSlot.NewAdjustmentSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAdjustmentSlot(AdjustmentSlot item)
    {
        return BenchmarkAuditPolicy.NormalizeBenchmarkAuditCode(item.Code);
    }
}
