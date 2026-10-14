namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PrimaryRefreshVarianceChecks
{
    public static void PrimaryRefreshVarianceStartsInDraft()
    {
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
        Check.Equal(item.Status, PrimaryRefreshVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRefreshVarianceMovesToClosed()
    {
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
        try
        {
            item.MoveTo(PrimaryRefreshVarianceStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRefreshVarianceStatus.Closed, "status");
    }

    public static void PrimaryRefreshVarianceRejectsMovesFromClosed()
    {
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
        item.Status = PrimaryRefreshVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryRefreshVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryRefreshVarianceStartsInDraft();
        PrimaryRefreshVarianceMovesToClosed();
        PrimaryRefreshVarianceRejectsMovesFromClosed();
    }
}
