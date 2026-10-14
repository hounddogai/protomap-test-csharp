namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TieredPayBandDraftChecks
{
    public static void TieredPayBandDraftStartsInPlanned()
    {
        var item = TieredPayBandDraft.NewTieredPayBandDraft("code-118", "label-107");
        Check.Equal(item.Status, TieredPayBandDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayBandDraftMovesToInProgress()
    {
        var item = TieredPayBandDraft.NewTieredPayBandDraft("code-118", "label-107");
        try
        {
            item.MoveTo(TieredPayBandDraftStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayBandDraftStatus.InProgress, "status");
    }

    public static void TieredPayBandDraftRejectsMovesFromCancelled()
    {
        var item = TieredPayBandDraft.NewTieredPayBandDraft("code-118", "label-107");
        item.Status = TieredPayBandDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredPayBandDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredPayBandDraftStartsInPlanned();
        TieredPayBandDraftMovesToInProgress();
        TieredPayBandDraftRejectsMovesFromCancelled();
    }
}
