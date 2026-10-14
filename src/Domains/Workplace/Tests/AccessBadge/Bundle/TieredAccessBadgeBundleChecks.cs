namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccessBadgeBundleChecks
{
    public static void TieredAccessBadgeBundleStartsInPlanned()
    {
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("code-18", "title-43");
        Check.Equal(item.Status, TieredAccessBadgeBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAccessBadgeBundleMovesToCompleted()
    {
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("code-18", "title-43");
        try
        {
            item.MoveTo(TieredAccessBadgeBundleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAccessBadgeBundleStatus.Completed, "status");
    }

    public static void TieredAccessBadgeBundleRejectsMovesFromCancelled()
    {
        var item = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle("code-18", "title-43");
        item.Status = TieredAccessBadgeBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredAccessBadgeBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredAccessBadgeBundleStartsInPlanned();
        TieredAccessBadgeBundleMovesToCompleted();
        TieredAccessBadgeBundleRejectsMovesFromCancelled();
    }
}
