namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMappingChecks
{
    public static void AccessBadgeMappingStartsInNew()
    {
        var item = AccessBadgeMapping.NewAccessBadgeMapping("code-779", "name-618");
        Check.Equal(item.Status, AccessBadgeMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeMappingMovesToRunning()
    {
        var item = AccessBadgeMapping.NewAccessBadgeMapping("code-779", "name-618");
        try
        {
            item.MoveTo(AccessBadgeMappingStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeMappingStatus.Running, "status");
    }

    public static void AccessBadgeMappingRejectsMovesFromFailed()
    {
        var item = AccessBadgeMapping.NewAccessBadgeMapping("code-779", "name-618");
        item.Status = AccessBadgeMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(AccessBadgeMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        AccessBadgeMappingStartsInNew();
        AccessBadgeMappingMovesToRunning();
        AccessBadgeMappingRejectsMovesFromFailed();
    }
}
