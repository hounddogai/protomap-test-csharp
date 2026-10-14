namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallAssignmentChecks
{
    public static void TownHallAssignmentStartsInPlanned()
    {
        var item = TownHallAssignment.NewTownHallAssignment("code-326", "title-163");
        Check.Equal(item.Status, TownHallAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallAssignmentMovesToScheduled()
    {
        var item = TownHallAssignment.NewTownHallAssignment("code-326", "title-163");
        try
        {
            item.MoveTo(TownHallAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallAssignmentStatus.Scheduled, "status");
    }

    public static void TownHallAssignmentRejectsMovesFromCancelled()
    {
        var item = TownHallAssignment.NewTownHallAssignment("code-326", "title-163");
        item.Status = TownHallAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TownHallAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TownHallAssignmentStartsInPlanned();
        TownHallAssignmentMovesToScheduled();
        TownHallAssignmentRejectsMovesFromCancelled();
    }
}
