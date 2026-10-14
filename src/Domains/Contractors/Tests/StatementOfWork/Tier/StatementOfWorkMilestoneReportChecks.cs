namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkMilestoneReportChecks
{
    public static void StatementOfWorkMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709238467);
        var items = new List<StatementOfWorkMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StatementOfWorkMilestone.NewStatementOfWorkMilestone($"statement-of-work-milestone-{index}", "sample"));
        }
        var report = StatementOfWorkMilestoneReport.BuildStatementOfWorkMilestoneReport("test", items, clock);
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
        StatementOfWorkMilestoneReportCountsEachRecordOnce();
    }
}
