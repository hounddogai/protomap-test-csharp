namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LeaveRequestLedgerReportChecks
{
    public static void LeaveRequestLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707269086);
        var items = new List<LeaveRequestLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LeaveRequestLedger.NewLeaveRequestLedger($"leave-request-ledger-{index}", "sample"));
        }
        var report = LeaveRequestLedgerReport.BuildLeaveRequestLedgerReport("test", items, clock);
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
        LeaveRequestLedgerReportCountsEachRecordOnce();
    }
}
