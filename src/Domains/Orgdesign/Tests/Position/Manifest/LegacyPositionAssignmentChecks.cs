namespace Acme.Workforce.Orgdesign.Tests.Position.Manifest;

using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LegacyPositionAssignmentChecks
{
    public static void LegacyPositionAssignmentStartsInDraft()
    {
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("code-352", "title-542");
        Check.Equal(item.Status, LegacyPositionAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionAssignmentMovesToClosed()
    {
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("code-352", "title-542");
        try
        {
            item.MoveTo(LegacyPositionAssignmentStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionAssignmentStatus.Closed, "status");
    }

    public static void LegacyPositionAssignmentRejectsMovesFromClosed()
    {
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("code-352", "title-542");
        item.Status = LegacyPositionAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyPositionAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyPositionAssignmentStartsInDraft();
        LegacyPositionAssignmentMovesToClosed();
        LegacyPositionAssignmentRejectsMovesFromClosed();
    }
}
