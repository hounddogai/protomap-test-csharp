namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBonusPoolDraftChecks
{
    public static void ManualBonusPoolDraftStartsInRequested()
    {
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("code-986", "name-756");
        Check.Equal(item.Status, ManualBonusPoolDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBonusPoolDraftMovesToGranted()
    {
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("code-986", "name-756");
        try
        {
            item.MoveTo(ManualBonusPoolDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBonusPoolDraftStatus.Granted, "status");
    }

    public static void ManualBonusPoolDraftRejectsMovesFromExpired()
    {
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("code-986", "name-756");
        item.Status = ManualBonusPoolDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualBonusPoolDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualBonusPoolDraftStartsInRequested();
        ManualBonusPoolDraftMovesToGranted();
        ManualBonusPoolDraftRejectsMovesFromExpired();
    }
}
