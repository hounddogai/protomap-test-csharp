namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class VisitorForecastChecks
{
    public static void VisitorForecastStartsInProposed()
    {
        var item = VisitorForecast.NewVisitorForecast("code-845", "name-989");
        Check.Equal(item.Status, VisitorForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorForecastMovesToApproved()
    {
        var item = VisitorForecast.NewVisitorForecast("code-845", "name-989");
        try
        {
            item.MoveTo(VisitorForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorForecastStatus.Approved, "status");
    }

    public static void VisitorForecastRejectsMovesFromWithdrawn()
    {
        var item = VisitorForecast.NewVisitorForecast("code-845", "name-989");
        item.Status = VisitorForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VisitorForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VisitorForecastStartsInProposed();
        VisitorForecastMovesToApproved();
        VisitorForecastRejectsMovesFromWithdrawn();
    }
}
