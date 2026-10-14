namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyTownHallAssignmentChecks
{
    public static void LegacyTownHallAssignmentStartsInIdle()
    {
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("code-254", "label-490");
        Check.Equal(item.Status, LegacyTownHallAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallAssignmentMovesToReady()
    {
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("code-254", "label-490");
        try
        {
            item.MoveTo(LegacyTownHallAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallAssignmentStatus.Ready, "status");
    }

    public static void LegacyTownHallAssignmentRejectsMovesFromStopped()
    {
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("code-254", "label-490");
        item.Status = LegacyTownHallAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyTownHallAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallAssignmentStartsInIdle();
        LegacyTownHallAssignmentMovesToReady();
        LegacyTownHallAssignmentRejectsMovesFromStopped();
    }
}
