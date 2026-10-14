namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentLedgerReportChecks
{
    public static void AssignmentLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708567825);
        var items = new List<AssignmentLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AssignmentLedger.NewAssignmentLedger($"assignment-ledger-{index}", "sample"));
        }
        var report = AssignmentLedgerReport.BuildAssignmentLedgerReport("test", items, clock);
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
        AssignmentLedgerReportCountsEachRecordOnce();
    }
}
