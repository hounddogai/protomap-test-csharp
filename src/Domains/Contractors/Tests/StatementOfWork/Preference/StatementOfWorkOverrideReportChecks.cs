namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideReportChecks
{
    public static void StatementOfWorkOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706618656);
        var items = new List<StatementOfWorkOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StatementOfWorkOverride.NewStatementOfWorkOverride($"statement-of-work-override-{index}", "sample"));
        }
        var report = StatementOfWorkOverrideReport.BuildStatementOfWorkOverrideReport("test", items, clock);
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
        StatementOfWorkOverrideReportCountsEachRecordOnce();
    }
}
