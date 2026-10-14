namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Functions of benchmark estimate from template.</summary>
public static class BenchmarkEstimateFromTemplate
{
    public static Dictionary<string, double> SummarizePayBandTemplatesForEstimate(List<PayBandTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<BenchmarkEstimate> DeriveBenchmarkEstimatesFromTemplate(List<PayBandTemplate> items)
    {
        var outValue = new List<BenchmarkEstimate>();
        foreach (var item in items)
        {
            var derived = BenchmarkEstimate.NewBenchmarkEstimate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectBenchmarkEstimate(BenchmarkEstimate item)
    {
        return PayBandTemplatePolicy.PayBandTemplateWeight(3, true);
    }
}
