namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalSpanEntryChecks
{
    public static void GlobalSpanEntryStartsInPlanned()
    {
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
        Check.Equal(item.Status, GlobalSpanEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSpanEntryMovesToCompleted()
    {
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
        try
        {
            item.MoveTo(GlobalSpanEntryStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSpanEntryStatus.Completed, "status");
    }

    public static void GlobalSpanEntryRejectsMovesFromCancelled()
    {
        var item = GlobalSpanEntry.NewGlobalSpanEntry("code-524", "name-922");
        item.Status = GlobalSpanEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalSpanEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalSpanEntryStartsInPlanned();
        GlobalSpanEntryMovesToCompleted();
        GlobalSpanEntryRejectsMovesFromCancelled();
    }
}
