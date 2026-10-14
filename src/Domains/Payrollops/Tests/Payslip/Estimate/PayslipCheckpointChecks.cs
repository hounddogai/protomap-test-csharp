namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipCheckpointChecks
{
    public static void PayslipCheckpointStartsInCreated()
    {
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
        Check.Equal(item.Status, PayslipCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipCheckpointMovesToPublished()
    {
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
        try
        {
            item.MoveTo(PayslipCheckpointStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipCheckpointStatus.Published, "status");
    }

    public static void PayslipCheckpointRejectsMovesFromRetired()
    {
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
        item.Status = PayslipCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(PayslipCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayslipCheckpointStartsInCreated();
        PayslipCheckpointMovesToPublished();
        PayslipCheckpointRejectsMovesFromRetired();
    }
}
