namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPositionProposalReportChecks
{
    public static void TieredPositionProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709691027);
        var items = new List<TieredPositionProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredPositionProposal.NewTieredPositionProposal($"tiered-position-proposal-{index}", "sample"));
        }
        var report = TieredPositionProposalReport.BuildTieredPositionProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredPositionProposalReportCountsEachRecordOnce();
    }
}
