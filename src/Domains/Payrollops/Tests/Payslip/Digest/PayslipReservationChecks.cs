namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipReservationChecks
{
    public static void PayslipReservationStartsInRequested()
    {
        var item = PayslipReservation.NewPayslipReservation("code-666", "name-430");
        Check.Equal(item.Status, PayslipReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipReservationMovesToRevoked()
    {
        var item = PayslipReservation.NewPayslipReservation("code-666", "name-430");
        try
        {
            item.MoveTo(PayslipReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipReservationStatus.Revoked, "status");
    }

    public static void PayslipReservationRejectsMovesFromExpired()
    {
        var item = PayslipReservation.NewPayslipReservation("code-666", "name-430");
        item.Status = PayslipReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipReservationStartsInRequested();
        PayslipReservationMovesToRevoked();
        PayslipReservationRejectsMovesFromExpired();
    }
}
