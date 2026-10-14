namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionProposalReportChecks
{
    public static void DeductionProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708780950);
        var items = new List<DeductionProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeductionProposal.NewDeductionProposal($"deduction-proposal-{index}", "sample"));
        }
        var report = DeductionProposalReport.BuildDeductionProposalReport("test", items, clock);
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
        DeductionProposalReportCountsEachRecordOnce();
    }
}
