namespace Acme.Workforce.Absence.Tests.Accrual.AccrualManifest;

using Acme.Workforce.Absence.Accrual.AccrualManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of accrual manifest code, run by the project's runner.</summary>
public static class PrimaryAccrualManifestReportChecks
{
    public static void PrimaryAccrualManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706834057);
        var items = new List<PrimaryAccrualManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryAccrualManifest.NewPrimaryAccrualManifest($"primary-accrual-manifest-{index}", "sample"));
        }
        var report = PrimaryAccrualManifestReport.BuildPrimaryAccrualManifestReport("test", items, clock);
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
        PrimaryAccrualManifestReportCountsEachRecordOnce();
    }
}
