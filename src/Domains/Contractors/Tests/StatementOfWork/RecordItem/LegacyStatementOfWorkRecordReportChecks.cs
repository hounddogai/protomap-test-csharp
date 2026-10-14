namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkRecordReportChecks
{
    public static void LegacyStatementOfWorkRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701507918);
        var items = new List<LegacyStatementOfWorkRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord($"legacy-statement-of-work-record-{index}", "sample"));
        }
        var report = LegacyStatementOfWorkRecordReport.BuildLegacyStatementOfWorkRecordReport("test", items, clock);
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
        LegacyStatementOfWorkRecordReportCountsEachRecordOnce();
    }
}
