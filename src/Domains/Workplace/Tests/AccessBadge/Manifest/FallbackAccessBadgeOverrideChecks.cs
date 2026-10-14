namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackAccessBadgeOverrideChecks
{
    public static void FallbackAccessBadgeOverrideStartsInNew()
    {
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("code-278", "title-640");
        Check.Equal(item.Status, FallbackAccessBadgeOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAccessBadgeOverrideMovesToFailed()
    {
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("code-278", "title-640");
        try
        {
            item.MoveTo(FallbackAccessBadgeOverrideStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAccessBadgeOverrideStatus.Failed, "status");
    }

    public static void FallbackAccessBadgeOverrideRejectsMovesFromFailed()
    {
        var item = FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride("code-278", "title-640");
        item.Status = FallbackAccessBadgeOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackAccessBadgeOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeOverrideStartsInNew();
        FallbackAccessBadgeOverrideMovesToFailed();
        FallbackAccessBadgeOverrideRejectsMovesFromFailed();
    }
}
