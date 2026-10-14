namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentTierChecks
{
    public static void AdjustmentTierStartsInCreated()
    {
        var item = AdjustmentTier.NewAdjustmentTier("code-77", "title-971");
        Check.Equal(item.Status, AdjustmentTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentTierMovesToValidated()
    {
        var item = AdjustmentTier.NewAdjustmentTier("code-77", "title-971");
        try
        {
            item.MoveTo(AdjustmentTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentTierStatus.Validated, "status");
    }

    public static void AdjustmentTierRejectsMovesFromRetired()
    {
        var item = AdjustmentTier.NewAdjustmentTier("code-77", "title-971");
        item.Status = AdjustmentTierStatus.Retired;
        Check.Fails(() => item.MoveTo(AdjustmentTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdjustmentTierStartsInCreated();
        AdjustmentTierMovesToValidated();
        AdjustmentTierRejectsMovesFromRetired();
    }
}
