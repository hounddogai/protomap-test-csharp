namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAllocationChecks
{
    public static void TaxFormAllocationStartsInCreated()
    {
        var item = TaxFormAllocation.NewTaxFormAllocation("code-400", "label-623");
        Check.Equal(item.Status, TaxFormAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormAllocationMovesToPublished()
    {
        var item = TaxFormAllocation.NewTaxFormAllocation("code-400", "label-623");
        try
        {
            item.MoveTo(TaxFormAllocationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormAllocationStatus.Published, "status");
    }

    public static void TaxFormAllocationRejectsMovesFromRetired()
    {
        var item = TaxFormAllocation.NewTaxFormAllocation("code-400", "label-623");
        item.Status = TaxFormAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(TaxFormAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        TaxFormAllocationStartsInCreated();
        TaxFormAllocationMovesToPublished();
        TaxFormAllocationRejectsMovesFromRetired();
    }
}
