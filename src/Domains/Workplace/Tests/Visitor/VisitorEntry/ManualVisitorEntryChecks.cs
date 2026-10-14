namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class ManualVisitorEntryChecks
{
    public static void ManualVisitorEntryStartsInProposed()
    {
        var item = ManualVisitorEntry.NewManualVisitorEntry("code-22", "label-703");
        Check.Equal(item.Status, ManualVisitorEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualVisitorEntryMovesToRejected()
    {
        var item = ManualVisitorEntry.NewManualVisitorEntry("code-22", "label-703");
        try
        {
            item.MoveTo(ManualVisitorEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualVisitorEntryStatus.Rejected, "status");
    }

    public static void ManualVisitorEntryRejectsMovesFromWithdrawn()
    {
        var item = ManualVisitorEntry.NewManualVisitorEntry("code-22", "label-703");
        item.Status = ManualVisitorEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualVisitorEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualVisitorEntryStartsInProposed();
        ManualVisitorEntryMovesToRejected();
        ManualVisitorEntryRejectsMovesFromWithdrawn();
    }
}
