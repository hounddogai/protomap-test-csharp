namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalReportChecks
{
    public static void GlobalPayslipProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703943565);
        var items = new List<GlobalPayslipProposal>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalPayslipProposal.NewGlobalPayslipProposal($"global-payslip-proposal-{index}", "sample"));
        }
        var report = GlobalPayslipProposalReport.BuildGlobalPayslipProposalReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalPayslipProposalReportCountsEachRecordOnce();
    }
}
