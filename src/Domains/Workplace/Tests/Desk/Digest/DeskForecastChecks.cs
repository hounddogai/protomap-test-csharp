namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskForecastChecks
{
    public static void DeskForecastStartsInPlanned()
    {
        var item = DeskForecast.NewDeskForecast("code-116", "title-560");
        Check.Equal(item.Status, DeskForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskForecastMovesToInProgress()
    {
        var item = DeskForecast.NewDeskForecast("code-116", "title-560");
        try
        {
            item.MoveTo(DeskForecastStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskForecastStatus.InProgress, "status");
    }

    public static void DeskForecastRejectsMovesFromCancelled()
    {
        var item = DeskForecast.NewDeskForecast("code-116", "title-560");
        item.Status = DeskForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DeskForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DeskForecastStartsInPlanned();
        DeskForecastMovesToInProgress();
        DeskForecastRejectsMovesFromCancelled();
    }
}
