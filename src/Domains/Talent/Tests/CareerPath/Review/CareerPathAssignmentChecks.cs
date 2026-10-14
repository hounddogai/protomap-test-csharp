namespace Acme.Workforce.Talent.Tests.CareerPath.Review;

using Acme.Workforce.Talent.CareerPath.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CareerPathAssignmentChecks
{
    public static void CareerPathAssignmentStartsInIdle()
    {
        var item = CareerPathAssignment.NewCareerPathAssignment("code-607", "name-71");
        Check.Equal(item.Status, CareerPathAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathAssignmentMovesToDraining()
    {
        var item = CareerPathAssignment.NewCareerPathAssignment("code-607", "name-71");
        try
        {
            item.MoveTo(CareerPathAssignmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathAssignmentStatus.Draining, "status");
    }

    public static void CareerPathAssignmentRejectsMovesFromStopped()
    {
        var item = CareerPathAssignment.NewCareerPathAssignment("code-607", "name-71");
        item.Status = CareerPathAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(CareerPathAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CareerPathAssignmentStartsInIdle();
        CareerPathAssignmentMovesToDraining();
        CareerPathAssignmentRejectsMovesFromStopped();
    }
}
