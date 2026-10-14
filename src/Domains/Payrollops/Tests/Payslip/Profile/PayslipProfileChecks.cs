namespace Acme.Workforce.Payrollops.Tests.Payslip.Profile;

using Acme.Workforce.Payrollops.Payslip.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PayslipProfileChecks
{
    public static void PayslipProfileStartsInProposed()
    {
        var item = PayslipProfile.NewPayslipProfile("code-434", "title-533");
        Check.Equal(item.Status, PayslipProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipProfileMovesToApproved()
    {
        var item = PayslipProfile.NewPayslipProfile("code-434", "title-533");
        try
        {
            item.MoveTo(PayslipProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipProfileStatus.Approved, "status");
    }

    public static void PayslipProfileRejectsMovesFromWithdrawn()
    {
        var item = PayslipProfile.NewPayslipProfile("code-434", "title-533");
        item.Status = PayslipProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayslipProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayslipProfileStartsInProposed();
        PayslipProfileMovesToApproved();
        PayslipProfileRejectsMovesFromWithdrawn();
    }
}
