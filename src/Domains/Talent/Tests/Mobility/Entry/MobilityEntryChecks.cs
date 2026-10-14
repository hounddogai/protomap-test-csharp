namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class MobilityEntryChecks
{
    public static void MobilityEntryStartsInDraft()
    {
        var item = MobilityEntry.NewMobilityEntry("code-650", "name-203");
        Check.Equal(item.Status, MobilityEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityEntryMovesToPending()
    {
        var item = MobilityEntry.NewMobilityEntry("code-650", "name-203");
        try
        {
            item.MoveTo(MobilityEntryStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityEntryStatus.Pending, "status");
    }

    public static void MobilityEntryRejectsMovesFromClosed()
    {
        var item = MobilityEntry.NewMobilityEntry("code-650", "name-203");
        item.Status = MobilityEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(MobilityEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MobilityEntryStartsInDraft();
        MobilityEntryMovesToPending();
        MobilityEntryRejectsMovesFromClosed();
    }
}
