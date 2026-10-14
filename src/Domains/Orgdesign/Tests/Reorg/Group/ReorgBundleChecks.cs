namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgBundleChecks
{
    public static void ReorgBundleStartsInDraft()
    {
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
        Check.Equal(item.Status, ReorgBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgBundleMovesToPending()
    {
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
        try
        {
            item.MoveTo(ReorgBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgBundleStatus.Pending, "status");
    }

    public static void ReorgBundleRejectsMovesFromClosed()
    {
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
        item.Status = ReorgBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(ReorgBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ReorgBundleStartsInDraft();
        ReorgBundleMovesToPending();
        ReorgBundleRejectsMovesFromClosed();
    }
}
