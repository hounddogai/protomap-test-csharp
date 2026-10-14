namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionTierChecks
{
    public static void LenientDeductionTierStartsInCreated()
    {
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
        Check.Equal(item.Status, LenientDeductionTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeductionTierMovesToValidated()
    {
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
        try
        {
            item.MoveTo(LenientDeductionTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeductionTierStatus.Validated, "status");
    }

    public static void LenientDeductionTierRejectsMovesFromRetired()
    {
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
        item.Status = LenientDeductionTierStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientDeductionTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientDeductionTierStartsInCreated();
        LenientDeductionTierMovesToValidated();
        LenientDeductionTierRejectsMovesFromRetired();
    }
}
