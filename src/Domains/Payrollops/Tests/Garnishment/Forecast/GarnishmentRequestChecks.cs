namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRequestChecks
{
    public static void GarnishmentRequestStartsInIdle()
    {
        var item = GarnishmentRequest.NewGarnishmentRequest("code-309", "title-295");
        Check.Equal(item.Status, GarnishmentRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentRequestMovesToDraining()
    {
        var item = GarnishmentRequest.NewGarnishmentRequest("code-309", "title-295");
        try
        {
            item.MoveTo(GarnishmentRequestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentRequestStatus.Draining, "status");
    }

    public static void GarnishmentRequestRejectsMovesFromStopped()
    {
        var item = GarnishmentRequest.NewGarnishmentRequest("code-309", "title-295");
        item.Status = GarnishmentRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(GarnishmentRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GarnishmentRequestStartsInIdle();
        GarnishmentRequestMovesToDraining();
        GarnishmentRequestRejectsMovesFromStopped();
    }
}
