namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class ManualBonusPoolProposalReportChecks
{
    public static void ManualBonusPoolProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701440057);
        var items = new List<ManualBonusPoolProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualBonusPoolProposal.NewManualBonusPoolProposal($"manual-bonus-pool-proposal-{index}", "sample"));
        }
        var report = ManualBonusPoolProposalReport.BuildManualBonusPoolProposalReport("test", items, clock);
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
        ManualBonusPoolProposalReportCountsEachRecordOnce();
    }
}
