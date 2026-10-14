namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkManifestReportChecks
{
    public static void StatementOfWorkManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703055425);
        var items = new List<StatementOfWorkManifest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StatementOfWorkManifest.NewStatementOfWorkManifest($"statement-of-work-manifest-{index}", "sample"));
        }
        var report = StatementOfWorkManifestReport.BuildStatementOfWorkManifestReport("test", items, clock);
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
        StatementOfWorkManifestReportCountsEachRecordOnce();
    }
}
