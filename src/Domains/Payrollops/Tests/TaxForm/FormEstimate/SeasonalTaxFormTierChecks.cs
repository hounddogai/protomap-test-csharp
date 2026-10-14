namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class SeasonalTaxFormTierChecks
{
    public static void SeasonalTaxFormTierStartsInCreated()
    {
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
        Check.Equal(item.Status, SeasonalTaxFormTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTaxFormTierMovesToPublished()
    {
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
        try
        {
            item.MoveTo(SeasonalTaxFormTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTaxFormTierStatus.Published, "status");
    }

    public static void SeasonalTaxFormTierRejectsMovesFromRetired()
    {
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
        item.Status = SeasonalTaxFormTierStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalTaxFormTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalTaxFormTierStartsInCreated();
        SeasonalTaxFormTierMovesToPublished();
        SeasonalTaxFormTierRejectsMovesFromRetired();
    }
}
