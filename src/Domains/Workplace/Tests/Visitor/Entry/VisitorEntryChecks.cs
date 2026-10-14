namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class VisitorEntryChecks
{
    public static void VisitorEntryStartsInNew()
    {
        var item = VisitorEntry.NewVisitorEntry("code-70", "title-947");
        Check.Equal(item.Status, VisitorEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorEntryMovesToFailed()
    {
        var item = VisitorEntry.NewVisitorEntry("code-70", "title-947");
        try
        {
            item.MoveTo(VisitorEntryStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorEntryStatus.Failed, "status");
    }

    public static void VisitorEntryRejectsMovesFromFailed()
    {
        var item = VisitorEntry.NewVisitorEntry("code-70", "title-947");
        item.Status = VisitorEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorEntryStartsInNew();
        VisitorEntryMovesToFailed();
        VisitorEntryRejectsMovesFromFailed();
    }
}
