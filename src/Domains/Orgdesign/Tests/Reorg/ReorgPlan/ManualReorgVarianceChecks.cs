namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class ManualReorgVarianceChecks
{
    public static void ManualReorgVarianceStartsInDraft()
    {
        var item = ManualReorgVariance.NewManualReorgVariance("code-192", "name-703");
        Check.Equal(item.Status, ManualReorgVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualReorgVarianceMovesToPending()
    {
        var item = ManualReorgVariance.NewManualReorgVariance("code-192", "name-703");
        try
        {
            item.MoveTo(ManualReorgVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualReorgVarianceStatus.Pending, "status");
    }

    public static void ManualReorgVarianceRejectsMovesFromClosed()
    {
        var item = ManualReorgVariance.NewManualReorgVariance("code-192", "name-703");
        item.Status = ManualReorgVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualReorgVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualReorgVarianceStartsInDraft();
        ManualReorgVarianceMovesToPending();
        ManualReorgVarianceRejectsMovesFromClosed();
    }
}
