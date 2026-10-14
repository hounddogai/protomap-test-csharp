namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class PendingReorgVarianceChecks
{
    public static void PendingReorgVarianceStartsInNew()
    {
        var item = PendingReorgVariance.NewPendingReorgVariance("code-951", "name-387");
        Check.Equal(item.Status, PendingReorgVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReorgVarianceMovesToDone()
    {
        var item = PendingReorgVariance.NewPendingReorgVariance("code-951", "name-387");
        try
        {
            item.MoveTo(PendingReorgVarianceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReorgVarianceStatus.Done, "status");
    }

    public static void PendingReorgVarianceRejectsMovesFromFailed()
    {
        var item = PendingReorgVariance.NewPendingReorgVariance("code-951", "name-387");
        item.Status = PendingReorgVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingReorgVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingReorgVarianceStartsInNew();
        PendingReorgVarianceMovesToDone();
        PendingReorgVarianceRejectsMovesFromFailed();
    }
}
