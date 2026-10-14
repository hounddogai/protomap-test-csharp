namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RollingBenchmarkProposalReportChecks
{
    public static void RollingBenchmarkProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708209099);
        var items = new List<RollingBenchmarkProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingBenchmarkProposal.NewRollingBenchmarkProposal($"rolling-benchmark-proposal-{index}", "sample"));
        }
        var report = RollingBenchmarkProposalReport.BuildRollingBenchmarkProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingBenchmarkProposalReportCountsEachRecordOnce();
    }
}
