namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolDraftReportChecks
{
    public static void BonusPoolDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700026078);
        var items = new List<BonusPoolDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BonusPoolDraft.NewBonusPoolDraft($"bonus-pool-draft-{index}", "sample"));
        }
        var report = BonusPoolDraftReport.BuildBonusPoolDraftReport("test", items, clock);
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
        BonusPoolDraftReportCountsEachRecordOnce();
    }
}
