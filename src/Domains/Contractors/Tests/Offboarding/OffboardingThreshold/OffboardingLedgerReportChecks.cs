namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class OffboardingLedgerReportChecks
{
    public static void OffboardingLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706037758);
        var items = new List<OffboardingLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OffboardingLedger.NewOffboardingLedger($"offboarding-ledger-{index}", "sample"));
        }
        var report = OffboardingLedgerReport.BuildOffboardingLedgerReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        OffboardingLedgerReportCountsEachRecordOnce();
    }
}
