namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalDeskVarianceChecks
{
    public static void GlobalDeskVarianceStartsInDraft()
    {
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
        Check.Equal(item.Status, GlobalDeskVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeskVarianceMovesToClosed()
    {
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
        try
        {
            item.MoveTo(GlobalDeskVarianceStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeskVarianceStatus.Closed, "status");
    }

    public static void GlobalDeskVarianceRejectsMovesFromClosed()
    {
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
        item.Status = GlobalDeskVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalDeskVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalDeskVarianceStartsInDraft();
        GlobalDeskVarianceMovesToClosed();
        GlobalDeskVarianceRejectsMovesFromClosed();
    }
}
