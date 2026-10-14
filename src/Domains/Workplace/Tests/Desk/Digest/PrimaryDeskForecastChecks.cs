namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastChecks
{
    public static void PrimaryDeskForecastStartsInOpen()
    {
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
        Check.Equal(item.Status, PrimaryDeskForecastStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryDeskForecastMovesToReopened()
    {
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
        try
        {
            item.MoveTo(PrimaryDeskForecastStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryDeskForecastStatus.Reopened, "status");
    }

    public static void PrimaryDeskForecastRejectsMovesFromArchived()
    {
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
        item.Status = PrimaryDeskForecastStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryDeskForecastStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryDeskForecastStartsInOpen();
        PrimaryDeskForecastMovesToReopened();
        PrimaryDeskForecastRejectsMovesFromArchived();
    }
}
