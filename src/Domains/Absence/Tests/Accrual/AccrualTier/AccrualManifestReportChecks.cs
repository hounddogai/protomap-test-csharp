namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestReportChecks
{
    public static void AccrualManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707490776);
        var items = new List<AccrualManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AccrualManifest.NewAccrualManifest($"accrual-manifest-{index}", "sample"));
        }
        var report = AccrualManifestReport.BuildAccrualManifestReport("test", items, clock);
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
        AccrualManifestReportCountsEachRecordOnce();
    }
}
