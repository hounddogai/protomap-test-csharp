namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentEntryChecks
{
    public static void AssignmentEntryStartsInProposed()
    {
        var item = AssignmentEntry.NewAssignmentEntry("code-555", "name-182");
        Check.Equal(item.Status, AssignmentEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentEntryMovesToApproved()
    {
        var item = AssignmentEntry.NewAssignmentEntry("code-555", "name-182");
        try
        {
            item.MoveTo(AssignmentEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentEntryStatus.Approved, "status");
    }

    public static void AssignmentEntryRejectsMovesFromWithdrawn()
    {
        var item = AssignmentEntry.NewAssignmentEntry("code-555", "name-182");
        item.Status = AssignmentEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssignmentEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssignmentEntryStartsInProposed();
        AssignmentEntryMovesToApproved();
        AssignmentEntryRejectsMovesFromWithdrawn();
    }
}
