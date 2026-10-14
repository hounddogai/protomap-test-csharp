namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalPayslipBaselineChecks
{
    public static void SeasonalPayslipBaselineStartsInPlanned()
    {
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("code-581", "label-999");
        Check.Equal(item.Status, SeasonalPayslipBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayslipBaselineMovesToScheduled()
    {
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("code-581", "label-999");
        try
        {
            item.MoveTo(SeasonalPayslipBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayslipBaselineStatus.Scheduled, "status");
    }

    public static void SeasonalPayslipBaselineRejectsMovesFromCancelled()
    {
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("code-581", "label-999");
        item.Status = SeasonalPayslipBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalPayslipBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalPayslipBaselineStartsInPlanned();
        SeasonalPayslipBaselineMovesToScheduled();
        SeasonalPayslipBaselineRejectsMovesFromCancelled();
    }
}
