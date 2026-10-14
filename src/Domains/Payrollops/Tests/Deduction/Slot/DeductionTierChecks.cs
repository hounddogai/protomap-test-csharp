namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionTierChecks
{
    public static void DeductionTierStartsInProposed()
    {
        var item = DeductionTier.NewDeductionTier("code-163", "title-291");
        Check.Equal(item.Status, DeductionTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionTierMovesToRejected()
    {
        var item = DeductionTier.NewDeductionTier("code-163", "title-291");
        try
        {
            item.MoveTo(DeductionTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionTierStatus.Rejected, "status");
    }

    public static void DeductionTierRejectsMovesFromWithdrawn()
    {
        var item = DeductionTier.NewDeductionTier("code-163", "title-291");
        item.Status = DeductionTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionTierStartsInProposed();
        DeductionTierMovesToRejected();
        DeductionTierRejectsMovesFromWithdrawn();
    }
}
