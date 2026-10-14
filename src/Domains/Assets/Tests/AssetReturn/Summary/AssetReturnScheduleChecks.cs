namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnScheduleChecks
{
    public static void AssetReturnScheduleStartsInPlanned()
    {
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
        Check.Equal(item.Status, AssetReturnScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnScheduleMovesToScheduled()
    {
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
        try
        {
            item.MoveTo(AssetReturnScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnScheduleStatus.Scheduled, "status");
    }

    public static void AssetReturnScheduleRejectsMovesFromCancelled()
    {
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
        item.Status = AssetReturnScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssetReturnScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssetReturnScheduleStartsInPlanned();
        AssetReturnScheduleMovesToScheduled();
        AssetReturnScheduleRejectsMovesFromCancelled();
    }
}
