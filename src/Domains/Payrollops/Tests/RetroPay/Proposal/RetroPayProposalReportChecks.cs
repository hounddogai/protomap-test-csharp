namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayProposalReportChecks
{
    public static void RetroPayProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708661322);
        var items = new List<RetroPayProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RetroPayProposal.NewRetroPayProposal($"retro-pay-proposal-{index}", "sample"));
        }
        var report = RetroPayProposalReport.BuildRetroPayProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RetroPayProposalReportCountsEachRecordOnce();
    }
}
