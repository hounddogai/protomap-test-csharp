namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionBatchChecks
{
    public static void PositionBatchStartsInRequested()
    {
        var item = PositionBatch.NewPositionBatch("code-876", "title-324");
        Check.Equal(item.Status, PositionBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionBatchMovesToGranted()
    {
        var item = PositionBatch.NewPositionBatch("code-876", "title-324");
        try
        {
            item.MoveTo(PositionBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionBatchStatus.Granted, "status");
    }

    public static void PositionBatchRejectsMovesFromExpired()
    {
        var item = PositionBatch.NewPositionBatch("code-876", "title-324");
        item.Status = PositionBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(PositionBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PositionBatchStartsInRequested();
        PositionBatchMovesToGranted();
        PositionBatchRejectsMovesFromExpired();
    }
}
