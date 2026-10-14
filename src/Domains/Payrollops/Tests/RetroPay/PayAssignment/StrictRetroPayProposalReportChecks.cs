namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class StrictRetroPayProposalReportChecks
{
    public static void StrictRetroPayProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709805441);
        var items = new List<StrictRetroPayProposal>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictRetroPayProposal.NewStrictRetroPayProposal($"strict-retro-pay-proposal-{index}", "sample"));
        }
        var report = StrictRetroPayProposalReport.BuildStrictRetroPayProposalReport("test", items, clock);
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
        StrictRetroPayProposalReportCountsEachRecordOnce();
    }
}
