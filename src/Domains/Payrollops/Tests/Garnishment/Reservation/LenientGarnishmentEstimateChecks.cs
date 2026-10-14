namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LenientGarnishmentEstimateChecks
{
    public static void LenientGarnishmentEstimateStartsInProposed()
    {
        var item = LenientGarnishmentEstimate.NewLenientGarnishmentEstimate("code-420", "name-802");
        Check.Equal(item.Status, LenientGarnishmentEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientGarnishmentEstimateMovesToApproved()
    {
        var item = LenientGarnishmentEstimate.NewLenientGarnishmentEstimate("code-420", "name-802");
        try
        {
            item.MoveTo(LenientGarnishmentEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientGarnishmentEstimateStatus.Approved, "status");
    }

    public static void LenientGarnishmentEstimateRejectsMovesFromWithdrawn()
    {
        var item = LenientGarnishmentEstimate.NewLenientGarnishmentEstimate("code-420", "name-802");
        item.Status = LenientGarnishmentEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientGarnishmentEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientGarnishmentEstimateStartsInProposed();
        LenientGarnishmentEstimateMovesToApproved();
        LenientGarnishmentEstimateRejectsMovesFromWithdrawn();
    }
}
