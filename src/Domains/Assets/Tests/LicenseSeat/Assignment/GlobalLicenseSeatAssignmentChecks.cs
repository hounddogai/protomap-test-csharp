namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalLicenseSeatAssignmentChecks
{
    public static void GlobalLicenseSeatAssignmentStartsInOpen()
    {
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
        Check.Equal(item.Status, GlobalLicenseSeatAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalLicenseSeatAssignmentMovesToResolved()
    {
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
        try
        {
            item.MoveTo(GlobalLicenseSeatAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalLicenseSeatAssignmentStatus.Resolved, "status");
    }

    public static void GlobalLicenseSeatAssignmentRejectsMovesFromArchived()
    {
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
        item.Status = GlobalLicenseSeatAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalLicenseSeatAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalLicenseSeatAssignmentStartsInOpen();
        GlobalLicenseSeatAssignmentMovesToResolved();
        GlobalLicenseSeatAssignmentRejectsMovesFromArchived();
    }
}
