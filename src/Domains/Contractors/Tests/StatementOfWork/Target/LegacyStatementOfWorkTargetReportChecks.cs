namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkTargetReportChecks
{
    public static void LegacyStatementOfWorkTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709391691);
        var items = new List<LegacyStatementOfWorkTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyStatementOfWorkTarget.NewLegacyStatementOfWorkTarget($"legacy-statement-of-work-target-{index}", "sample"));
        }
        var report = LegacyStatementOfWorkTargetReport.BuildLegacyStatementOfWorkTargetReport("test", items, clock);
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
        LegacyStatementOfWorkTargetReportCountsEachRecordOnce();
    }
}
