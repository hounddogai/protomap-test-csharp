namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TaxFormProfileChecks
{
    public static void TaxFormProfileStartsInPlanned()
    {
        var item = TaxFormProfile.NewTaxFormProfile("code-945", "name-76");
        Check.Equal(item.Status, TaxFormProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormProfileMovesToInProgress()
    {
        var item = TaxFormProfile.NewTaxFormProfile("code-945", "name-76");
        try
        {
            item.MoveTo(TaxFormProfileStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormProfileStatus.InProgress, "status");
    }

    public static void TaxFormProfileRejectsMovesFromCancelled()
    {
        var item = TaxFormProfile.NewTaxFormProfile("code-945", "name-76");
        item.Status = TaxFormProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormProfileStartsInPlanned();
        TaxFormProfileMovesToInProgress();
        TaxFormProfileRejectsMovesFromCancelled();
    }
}
