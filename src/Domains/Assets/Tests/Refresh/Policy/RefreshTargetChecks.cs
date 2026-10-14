namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshTargetChecks
{
    public static void RefreshTargetStartsInCreated()
    {
        var item = RefreshTarget.NewRefreshTarget("code-240", "title-36");
        Check.Equal(item.Status, RefreshTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshTargetMovesToPublished()
    {
        var item = RefreshTarget.NewRefreshTarget("code-240", "title-36");
        try
        {
            item.MoveTo(RefreshTargetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshTargetStatus.Published, "status");
    }

    public static void RefreshTargetRejectsMovesFromRetired()
    {
        var item = RefreshTarget.NewRefreshTarget("code-240", "title-36");
        item.Status = RefreshTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(RefreshTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        RefreshTargetStartsInCreated();
        RefreshTargetMovesToPublished();
        RefreshTargetRejectsMovesFromRetired();
    }
}
