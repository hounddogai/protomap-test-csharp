namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewReportChecks
{
    public static void LegacyMeritCycleReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700308473);
        var items = new List<LegacyMeritCycleReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyMeritCycleReview.NewLegacyMeritCycleReview($"legacy-merit-cycle-review-{index}", "sample"));
        }
        var report = LegacyMeritCycleReviewReport.BuildLegacyMeritCycleReviewReport("test", items, clock);
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
        LegacyMeritCycleReviewReportCountsEachRecordOnce();
    }
}
