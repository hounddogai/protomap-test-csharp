namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedAccessBadgeThresholdChecks
{
    public static void CappedAccessBadgeThresholdStartsInNew()
    {
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("code-931", "title-878");
        Check.Equal(item.Status, CappedAccessBadgeThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAccessBadgeThresholdMovesToFailed()
    {
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("code-931", "title-878");
        try
        {
            item.MoveTo(CappedAccessBadgeThresholdStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAccessBadgeThresholdStatus.Failed, "status");
    }

    public static void CappedAccessBadgeThresholdRejectsMovesFromFailed()
    {
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("code-931", "title-878");
        item.Status = CappedAccessBadgeThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedAccessBadgeThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedAccessBadgeThresholdStartsInNew();
        CappedAccessBadgeThresholdMovesToFailed();
        CappedAccessBadgeThresholdRejectsMovesFromFailed();
    }
}
