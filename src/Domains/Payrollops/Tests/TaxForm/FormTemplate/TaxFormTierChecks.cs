namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormTierChecks
{
    public static void TaxFormTierStartsInNew()
    {
        var item = TaxFormTier.NewTaxFormTier("code-187", "name-469");
        Check.Equal(item.Status, TaxFormTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormTierMovesToRunning()
    {
        var item = TaxFormTier.NewTaxFormTier("code-187", "name-469");
        try
        {
            item.MoveTo(TaxFormTierStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormTierStatus.Running, "status");
    }

    public static void TaxFormTierRejectsMovesFromFailed()
    {
        var item = TaxFormTier.NewTaxFormTier("code-187", "name-469");
        item.Status = TaxFormTierStatus.Failed;
        Check.Fails(() => item.MoveTo(TaxFormTierStatus.New), "move");
    }

    public static void RunAll()
    {
        TaxFormTierStartsInNew();
        TaxFormTierMovesToRunning();
        TaxFormTierRejectsMovesFromFailed();
    }
}
