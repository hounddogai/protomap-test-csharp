namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictTaxFormTierChecks
{
    public static void StrictTaxFormTierStartsInCreated()
    {
        var item = StrictTaxFormTier.NewStrictTaxFormTier("code-913", "label-939");
        Check.Equal(item.Status, StrictTaxFormTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTaxFormTierMovesToRetired()
    {
        var item = StrictTaxFormTier.NewStrictTaxFormTier("code-913", "label-939");
        try
        {
            item.MoveTo(StrictTaxFormTierStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTaxFormTierStatus.Retired, "status");
    }

    public static void StrictTaxFormTierRejectsMovesFromRetired()
    {
        var item = StrictTaxFormTier.NewStrictTaxFormTier("code-913", "label-939");
        item.Status = StrictTaxFormTierStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictTaxFormTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictTaxFormTierStartsInCreated();
        StrictTaxFormTierMovesToRetired();
        StrictTaxFormTierRejectsMovesFromRetired();
    }
}
