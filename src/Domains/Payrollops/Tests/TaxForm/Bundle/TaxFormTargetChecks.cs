namespace Acme.Workforce.Payrollops.Tests.TaxForm.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TaxFormTargetChecks
{
    public static void TaxFormTargetStartsInCreated()
    {
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
        Check.Equal(item.Status, TaxFormTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormTargetMovesToPublished()
    {
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
        try
        {
            item.MoveTo(TaxFormTargetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormTargetStatus.Published, "status");
    }

    public static void TaxFormTargetRejectsMovesFromRetired()
    {
        var item = TaxFormTarget.NewTaxFormTarget("code-679", "title-912");
        item.Status = TaxFormTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(TaxFormTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        TaxFormTargetStartsInCreated();
        TaxFormTargetMovesToPublished();
        TaxFormTargetRejectsMovesFromRetired();
    }
}
