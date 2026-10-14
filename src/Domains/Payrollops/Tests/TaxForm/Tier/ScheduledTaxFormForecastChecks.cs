namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledTaxFormForecastChecks
{
    public static void ScheduledTaxFormForecastStartsInProposed()
    {
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
        Check.Equal(item.Status, ScheduledTaxFormForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTaxFormForecastMovesToRejected()
    {
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
        try
        {
            item.MoveTo(ScheduledTaxFormForecastStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTaxFormForecastStatus.Rejected, "status");
    }

    public static void ScheduledTaxFormForecastRejectsMovesFromWithdrawn()
    {
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
        item.Status = ScheduledTaxFormForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledTaxFormForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledTaxFormForecastStartsInProposed();
        ScheduledTaxFormForecastMovesToRejected();
        ScheduledTaxFormForecastRejectsMovesFromWithdrawn();
    }
}
