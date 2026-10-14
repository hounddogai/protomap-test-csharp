namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateChecks
{
    public static void PayslipEstimateStartsInProposed()
    {
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
        Check.Equal(item.Status, PayslipEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipEstimateMovesToRejected()
    {
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
        try
        {
            item.MoveTo(PayslipEstimateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipEstimateStatus.Rejected, "status");
    }

    public static void PayslipEstimateRejectsMovesFromWithdrawn()
    {
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
        item.Status = PayslipEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayslipEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayslipEstimateStartsInProposed();
        PayslipEstimateMovesToRejected();
        PayslipEstimateRejectsMovesFromWithdrawn();
    }
}
