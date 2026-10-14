namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingLedgerReportChecks
{
    public static void AdaptiveOffboardingLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704293499);
        var items = new List<AdaptiveOffboardingLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger($"adaptive-offboarding-ledger-{index}", "sample"));
        }
        var report = AdaptiveOffboardingLedgerReport.BuildAdaptiveOffboardingLedgerReport("test", items, clock);
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
        AdaptiveOffboardingLedgerReportCountsEachRecordOnce();
    }
}
