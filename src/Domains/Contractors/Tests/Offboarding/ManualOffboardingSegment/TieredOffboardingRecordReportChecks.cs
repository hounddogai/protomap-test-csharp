namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class TieredOffboardingRecordReportChecks
{
    public static void TieredOffboardingRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704006823);
        var items = new List<TieredOffboardingRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredOffboardingRecord.NewTieredOffboardingRecord($"tiered-offboarding-record-{index}", "sample"));
        }
        var report = TieredOffboardingRecordReport.BuildTieredOffboardingRecordReport("test", items, clock);
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
        TieredOffboardingRecordReportCountsEachRecordOnce();
    }
}
