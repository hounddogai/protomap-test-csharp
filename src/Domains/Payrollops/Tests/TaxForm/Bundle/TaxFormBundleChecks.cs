namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormBundleChecks
{
    public static void TaxFormBundleStartsInDraft()
    {
        var item = TaxFormBundle.NewTaxFormBundle("code-223", "name-500");
        Check.Equal(item.Status, TaxFormBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormBundleMovesToPending()
    {
        var item = TaxFormBundle.NewTaxFormBundle("code-223", "name-500");
        try
        {
            item.MoveTo(TaxFormBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormBundleStatus.Pending, "status");
    }

    public static void TaxFormBundleRejectsMovesFromClosed()
    {
        var item = TaxFormBundle.NewTaxFormBundle("code-223", "name-500");
        item.Status = TaxFormBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormBundleStartsInDraft();
        TaxFormBundleMovesToPending();
        TaxFormBundleRejectsMovesFromClosed();
    }
}
