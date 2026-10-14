namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RefreshCheckpointChecks
{
    public static void RefreshCheckpointStartsInOpen()
    {
        var item = RefreshCheckpoint.NewRefreshCheckpoint("code-711", "title-152");
        Check.Equal(item.Status, RefreshCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshCheckpointMovesToReopened()
    {
        var item = RefreshCheckpoint.NewRefreshCheckpoint("code-711", "title-152");
        try
        {
            item.MoveTo(RefreshCheckpointStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshCheckpointStatus.Reopened, "status");
    }

    public static void RefreshCheckpointRejectsMovesFromArchived()
    {
        var item = RefreshCheckpoint.NewRefreshCheckpoint("code-711", "title-152");
        item.Status = RefreshCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(RefreshCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        RefreshCheckpointStartsInOpen();
        RefreshCheckpointMovesToReopened();
        RefreshCheckpointRejectsMovesFromArchived();
    }
}
