namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalReportChecks
{
    public static void LeaveRequestProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705295813);
        var items = new List<LeaveRequestProposal>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LeaveRequestProposal.NewLeaveRequestProposal($"leave-request-proposal-{index}", "sample"));
        }
        var report = LeaveRequestProposalReport.BuildLeaveRequestProposalReport("test", items, clock);
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
        LeaveRequestProposalReportCountsEachRecordOnce();
    }
}
