namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentForecastChecks
{
    public static void AssignmentForecastStartsInProposed()
    {
        var item = AssignmentForecast.NewAssignmentForecast("code-437", "title-958");
        Check.Equal(item.Status, AssignmentForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentForecastMovesToApproved()
    {
        var item = AssignmentForecast.NewAssignmentForecast("code-437", "title-958");
        try
        {
            item.MoveTo(AssignmentForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentForecastStatus.Approved, "status");
    }

    public static void AssignmentForecastRejectsMovesFromWithdrawn()
    {
        var item = AssignmentForecast.NewAssignmentForecast("code-437", "title-958");
        item.Status = AssignmentForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssignmentForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssignmentForecastStartsInProposed();
        AssignmentForecastMovesToApproved();
        AssignmentForecastRejectsMovesFromWithdrawn();
    }
}
