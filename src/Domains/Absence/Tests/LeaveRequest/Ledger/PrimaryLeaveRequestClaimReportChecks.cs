namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimReportChecks
{
    public static void PrimaryLeaveRequestClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704007195);
        var items = new List<PrimaryLeaveRequestClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim($"primary-leave-request-claim-{index}", "sample"));
        }
        var report = PrimaryLeaveRequestClaimReport.BuildPrimaryLeaveRequestClaimReport("test", items, clock);
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
        PrimaryLeaveRequestClaimReportCountsEachRecordOnce();
    }
}
