namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleProposalReportChecks
{
    public static void MeritCycleProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703686752);
        var items = new List<MeritCycleProposal>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(MeritCycleProposal.NewMeritCycleProposal($"merit-cycle-proposal-{index}", "sample"));
        }
        var report = MeritCycleProposalReport.BuildMeritCycleProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        MeritCycleProposalReportCountsEachRecordOnce();
    }
}
