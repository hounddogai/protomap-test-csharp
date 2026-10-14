namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Functions of benchmark milestone from proposal.</summary>
public static class BenchmarkMilestoneFromProposal
{
    public static Dictionary<string, double> SummarizeStrictMeritCycleProposalsForMilestone(List<StrictMeritCycleProposal> items)
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

    public static List<BenchmarkMilestone> DeriveBenchmarkMilestonesFromProposal(List<StrictMeritCycleProposal> items)
    {
        var outValue = new List<BenchmarkMilestone>();
        foreach (var item in items)
        {
            var derived = BenchmarkMilestone.NewBenchmarkMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBenchmarkMilestone(BenchmarkMilestone item)
    {
        return MeritCycleGroupPolicy.NormalizeMeritCycleGroupCode(item.Code);
    }
}
