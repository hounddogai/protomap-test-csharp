namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolProposalReportChecks
{
    public static void BonusPoolProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701944598);
        var items = new List<BonusPoolProposal>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BonusPoolProposal.NewBonusPoolProposal($"bonus-pool-proposal-{index}", "sample"));
        }
        var report = BonusPoolProposalReport.BuildBonusPoolProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BonusPoolProposalReportCountsEachRecordOnce();
    }
}
