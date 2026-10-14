namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleForecastChecks
{
    public static void MeritCycleForecastStartsInProposed()
    {
        var item = MeritCycleForecast.NewMeritCycleForecast("code-807", "title-23");
        Check.Equal(item.Status, MeritCycleForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleForecastMovesToApproved()
    {
        var item = MeritCycleForecast.NewMeritCycleForecast("code-807", "title-23");
        try
        {
            item.MoveTo(MeritCycleForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleForecastStatus.Approved, "status");
    }

    public static void MeritCycleForecastRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleForecast.NewMeritCycleForecast("code-807", "title-23");
        item.Status = MeritCycleForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleForecastStartsInProposed();
        MeritCycleForecastMovesToApproved();
        MeritCycleForecastRejectsMovesFromWithdrawn();
    }
}
