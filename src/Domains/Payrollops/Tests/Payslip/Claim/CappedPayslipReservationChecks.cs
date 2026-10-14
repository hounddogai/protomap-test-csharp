namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CappedPayslipReservationChecks
{
    public static void CappedPayslipReservationStartsInRequested()
    {
        var item = CappedPayslipReservation.NewCappedPayslipReservation("code-543", "label-527");
        Check.Equal(item.Status, CappedPayslipReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayslipReservationMovesToRevoked()
    {
        var item = CappedPayslipReservation.NewCappedPayslipReservation("code-543", "label-527");
        try
        {
            item.MoveTo(CappedPayslipReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayslipReservationStatus.Revoked, "status");
    }

    public static void CappedPayslipReservationRejectsMovesFromExpired()
    {
        var item = CappedPayslipReservation.NewCappedPayslipReservation("code-543", "label-527");
        item.Status = CappedPayslipReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedPayslipReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedPayslipReservationStartsInRequested();
        CappedPayslipReservationMovesToRevoked();
        CappedPayslipReservationRejectsMovesFromExpired();
    }
}
