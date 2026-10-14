namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Functions of global pay band agreement from request.</summary>
public static class GlobalPayBandAgreementFromRequest
{
    public static Dictionary<string, double> SummarizeBenchmarkRequestsForAgreement(List<BenchmarkRequest> items)
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

    public static List<GlobalPayBandAgreement> DeriveGlobalPayBandAgreementsFromRequest(List<BenchmarkRequest> items)
    {
        var outValue = new List<GlobalPayBandAgreement>();
        foreach (var item in items)
        {
            var derived = GlobalPayBandAgreement.NewGlobalPayBandAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectGlobalPayBandAgreement(GlobalPayBandAgreement item)
    {
        return BenchmarkRequestPolicy.ClassifyBenchmarkRequest(item.NetVolume());
    }
}
