namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class FallbackPositionRecordChecks
{
    public static void FallbackPositionRecordStartsInPlanned()
    {
        var item = FallbackPositionRecord.NewFallbackPositionRecord("code-288", "label-951");
        Check.Equal(item.Status, FallbackPositionRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPositionRecordMovesToCompleted()
    {
        var item = FallbackPositionRecord.NewFallbackPositionRecord("code-288", "label-951");
        try
        {
            item.MoveTo(FallbackPositionRecordStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPositionRecordStatus.Completed, "status");
    }

    public static void FallbackPositionRecordRejectsMovesFromCancelled()
    {
        var item = FallbackPositionRecord.NewFallbackPositionRecord("code-288", "label-951");
        item.Status = FallbackPositionRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPositionRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPositionRecordStartsInPlanned();
        FallbackPositionRecordMovesToCompleted();
        FallbackPositionRecordRejectsMovesFromCancelled();
    }
}
