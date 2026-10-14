namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormForecastChecks
{
    public static void TaxFormForecastStartsInRequested()
    {
        var item = TaxFormForecast.NewTaxFormForecast("code-685", "title-615");
        Check.Equal(item.Status, TaxFormForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormForecastMovesToGranted()
    {
        var item = TaxFormForecast.NewTaxFormForecast("code-685", "title-615");
        try
        {
            item.MoveTo(TaxFormForecastStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormForecastStatus.Granted, "status");
    }

    public static void TaxFormForecastRejectsMovesFromExpired()
    {
        var item = TaxFormForecast.NewTaxFormForecast("code-685", "title-615");
        item.Status = TaxFormForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(TaxFormForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TaxFormForecastStartsInRequested();
        TaxFormForecastMovesToGranted();
        TaxFormForecastRejectsMovesFromExpired();
    }
}
