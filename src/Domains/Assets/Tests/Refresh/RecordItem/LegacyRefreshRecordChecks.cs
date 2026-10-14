namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRefreshRecordChecks
{
    public static void LegacyRefreshRecordStartsInNew()
    {
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
        Check.Equal(item.Status, LegacyRefreshRecordStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRefreshRecordMovesToQueued()
    {
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
        try
        {
            item.MoveTo(LegacyRefreshRecordStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRefreshRecordStatus.Queued, "status");
    }

    public static void LegacyRefreshRecordRejectsMovesFromFailed()
    {
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("code-726", "label-838");
        item.Status = LegacyRefreshRecordStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyRefreshRecordStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyRefreshRecordStartsInNew();
        LegacyRefreshRecordMovesToQueued();
        LegacyRefreshRecordRejectsMovesFromFailed();
    }
}
