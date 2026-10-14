namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleChecks
{
    public static void TieredPositionBundleStartsInDraft()
    {
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
        Check.Equal(item.Status, TieredPositionBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPositionBundleMovesToActive()
    {
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
        try
        {
            item.MoveTo(TieredPositionBundleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPositionBundleStatus.Active, "status");
    }

    public static void TieredPositionBundleRejectsMovesFromClosed()
    {
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
        item.Status = TieredPositionBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredPositionBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredPositionBundleStartsInDraft();
        TieredPositionBundleMovesToActive();
        TieredPositionBundleRejectsMovesFromClosed();
    }
}
