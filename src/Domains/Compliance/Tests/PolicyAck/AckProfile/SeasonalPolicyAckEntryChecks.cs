namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEntryChecks
{
    public static void SeasonalPolicyAckEntryStartsInNew()
    {
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("code-887", "title-483");
        Check.Equal(item.Status, SeasonalPolicyAckEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPolicyAckEntryMovesToQueued()
    {
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("code-887", "title-483");
        try
        {
            item.MoveTo(SeasonalPolicyAckEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPolicyAckEntryStatus.Queued, "status");
    }

    public static void SeasonalPolicyAckEntryRejectsMovesFromFailed()
    {
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("code-887", "title-483");
        item.Status = SeasonalPolicyAckEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalPolicyAckEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckEntryStartsInNew();
        SeasonalPolicyAckEntryMovesToQueued();
        SeasonalPolicyAckEntryRejectsMovesFromFailed();
    }
}
