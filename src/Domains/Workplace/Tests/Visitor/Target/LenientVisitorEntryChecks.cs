namespace Acme.Workforce.Workplace.Tests.Visitor.Target;

using Acme.Workforce.Workplace.Visitor.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientVisitorEntryChecks
{
    public static void LenientVisitorEntryStartsInPlanned()
    {
        var item = LenientVisitorEntry.NewLenientVisitorEntry("code-699", "name-696");
        Check.Equal(item.Status, LenientVisitorEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVisitorEntryMovesToInProgress()
    {
        var item = LenientVisitorEntry.NewLenientVisitorEntry("code-699", "name-696");
        try
        {
            item.MoveTo(LenientVisitorEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVisitorEntryStatus.InProgress, "status");
    }

    public static void LenientVisitorEntryRejectsMovesFromCancelled()
    {
        var item = LenientVisitorEntry.NewLenientVisitorEntry("code-699", "name-696");
        item.Status = LenientVisitorEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientVisitorEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientVisitorEntryStartsInPlanned();
        LenientVisitorEntryMovesToInProgress();
        LenientVisitorEntryRejectsMovesFromCancelled();
    }
}
