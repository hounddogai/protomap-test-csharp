namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class AssignmentReservationChecks
{
    public static void AssignmentReservationStartsInCreated()
    {
        var item = AssignmentReservation.NewAssignmentReservation("code-172", "name-795");
        Check.Equal(item.Status, AssignmentReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentReservationMovesToPublished()
    {
        var item = AssignmentReservation.NewAssignmentReservation("code-172", "name-795");
        try
        {
            item.MoveTo(AssignmentReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentReservationStatus.Published, "status");
    }

    public static void AssignmentReservationRejectsMovesFromRetired()
    {
        var item = AssignmentReservation.NewAssignmentReservation("code-172", "name-795");
        item.Status = AssignmentReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(AssignmentReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssignmentReservationStartsInCreated();
        AssignmentReservationMovesToPublished();
        AssignmentReservationRejectsMovesFromRetired();
    }
}
