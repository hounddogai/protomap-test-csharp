namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultHeadcountEntryChecks
{
    public static void DefaultHeadcountEntryStartsInProposed()
    {
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
        Check.Equal(item.Status, DefaultHeadcountEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountEntryMovesToRejected()
    {
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
        try
        {
            item.MoveTo(DefaultHeadcountEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountEntryStatus.Rejected, "status");
    }

    public static void DefaultHeadcountEntryRejectsMovesFromWithdrawn()
    {
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
        item.Status = DefaultHeadcountEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultHeadcountEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountEntryStartsInProposed();
        DefaultHeadcountEntryMovesToRejected();
        DefaultHeadcountEntryRejectsMovesFromWithdrawn();
    }
}
