namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredPolicyAckEntryChecks
{
    public static void TieredPolicyAckEntryStartsInDraft()
    {
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("code-186", "title-633");
        Check.Equal(item.Status, TieredPolicyAckEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyAckEntryMovesToPending()
    {
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("code-186", "title-633");
        try
        {
            item.MoveTo(TieredPolicyAckEntryStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyAckEntryStatus.Pending, "status");
    }

    public static void TieredPolicyAckEntryRejectsMovesFromClosed()
    {
        var item = TieredPolicyAckEntry.NewTieredPolicyAckEntry("code-186", "title-633");
        item.Status = TieredPolicyAckEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredPolicyAckEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredPolicyAckEntryStartsInDraft();
        TieredPolicyAckEntryMovesToPending();
        TieredPolicyAckEntryRejectsMovesFromClosed();
    }
}
