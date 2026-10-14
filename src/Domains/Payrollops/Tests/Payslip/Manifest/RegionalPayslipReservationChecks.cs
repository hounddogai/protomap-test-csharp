namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class RegionalPayslipReservationChecks
{
    public static void RegionalPayslipReservationStartsInProposed()
    {
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("code-809", "label-200");
        Check.Equal(item.Status, RegionalPayslipReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayslipReservationMovesToApproved()
    {
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("code-809", "label-200");
        try
        {
            item.MoveTo(RegionalPayslipReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayslipReservationStatus.Approved, "status");
    }

    public static void RegionalPayslipReservationRejectsMovesFromWithdrawn()
    {
        var item = RegionalPayslipReservation.NewRegionalPayslipReservation("code-809", "label-200");
        item.Status = RegionalPayslipReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalPayslipReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalPayslipReservationStartsInProposed();
        RegionalPayslipReservationMovesToApproved();
        RegionalPayslipReservationRejectsMovesFromWithdrawn();
    }
}
