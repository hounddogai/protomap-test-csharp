namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultPayslipThresholdChecks
{
    public static void DefaultPayslipThresholdStartsInNew()
    {
        var item = DefaultPayslipThreshold.NewDefaultPayslipThreshold("code-36", "title-893");
        Check.Equal(item.Status, DefaultPayslipThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayslipThresholdMovesToQueued()
    {
        var item = DefaultPayslipThreshold.NewDefaultPayslipThreshold("code-36", "title-893");
        try
        {
            item.MoveTo(DefaultPayslipThresholdStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayslipThresholdStatus.Queued, "status");
    }

    public static void DefaultPayslipThresholdRejectsMovesFromFailed()
    {
        var item = DefaultPayslipThreshold.NewDefaultPayslipThreshold("code-36", "title-893");
        item.Status = DefaultPayslipThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultPayslipThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultPayslipThresholdStartsInNew();
        DefaultPayslipThresholdMovesToQueued();
        DefaultPayslipThresholdRejectsMovesFromFailed();
    }
}
