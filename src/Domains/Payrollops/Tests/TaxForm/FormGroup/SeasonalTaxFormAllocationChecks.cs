namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class SeasonalTaxFormAllocationChecks
{
    public static void SeasonalTaxFormAllocationStartsInIdle()
    {
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("code-891", "name-663");
        Check.Equal(item.Status, SeasonalTaxFormAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTaxFormAllocationMovesToWarming()
    {
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("code-891", "name-663");
        try
        {
            item.MoveTo(SeasonalTaxFormAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTaxFormAllocationStatus.Warming, "status");
    }

    public static void SeasonalTaxFormAllocationRejectsMovesFromStopped()
    {
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("code-891", "name-663");
        item.Status = SeasonalTaxFormAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalTaxFormAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalTaxFormAllocationStartsInIdle();
        SeasonalTaxFormAllocationMovesToWarming();
        SeasonalTaxFormAllocationRejectsMovesFromStopped();
    }
}
