namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingRefreshMappingChecks
{
    public static void PendingRefreshMappingStartsInPlanned()
    {
        var item = PendingRefreshMapping.NewPendingRefreshMapping("code-902", "label-311");
        Check.Equal(item.Status, PendingRefreshMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRefreshMappingMovesToInProgress()
    {
        var item = PendingRefreshMapping.NewPendingRefreshMapping("code-902", "label-311");
        try
        {
            item.MoveTo(PendingRefreshMappingStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRefreshMappingStatus.InProgress, "status");
    }

    public static void PendingRefreshMappingRejectsMovesFromCancelled()
    {
        var item = PendingRefreshMapping.NewPendingRefreshMapping("code-902", "label-311");
        item.Status = PendingRefreshMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingRefreshMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingRefreshMappingStartsInPlanned();
        PendingRefreshMappingMovesToInProgress();
        PendingRefreshMappingRejectsMovesFromCancelled();
    }
}
