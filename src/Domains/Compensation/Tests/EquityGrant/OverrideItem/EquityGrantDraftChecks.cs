namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantDraftChecks
{
    public static void EquityGrantDraftStartsInNew()
    {
        var item = EquityGrantDraft.NewEquityGrantDraft("code-349", "title-843");
        Check.Equal(item.Status, EquityGrantDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantDraftMovesToQueued()
    {
        var item = EquityGrantDraft.NewEquityGrantDraft("code-349", "title-843");
        try
        {
            item.MoveTo(EquityGrantDraftStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantDraftStatus.Queued, "status");
    }

    public static void EquityGrantDraftRejectsMovesFromFailed()
    {
        var item = EquityGrantDraft.NewEquityGrantDraft("code-349", "title-843");
        item.Status = EquityGrantDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(EquityGrantDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        EquityGrantDraftStartsInNew();
        EquityGrantDraftMovesToQueued();
        EquityGrantDraftRejectsMovesFromFailed();
    }
}
