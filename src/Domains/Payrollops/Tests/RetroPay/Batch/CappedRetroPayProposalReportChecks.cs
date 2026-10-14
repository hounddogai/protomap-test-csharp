namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedRetroPayProposalReportChecks
{
    public static void CappedRetroPayProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703334581);
        var items = new List<CappedRetroPayProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CappedRetroPayProposal.NewCappedRetroPayProposal($"capped-retro-pay-proposal-{index}", "sample"));
        }
        var report = CappedRetroPayProposalReport.BuildCappedRetroPayProposalReport("test", items, clock);
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
        CappedRetroPayProposalReportCountsEachRecordOnce();
    }
}
