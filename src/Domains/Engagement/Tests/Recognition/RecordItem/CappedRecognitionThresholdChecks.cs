namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedRecognitionThresholdChecks
{
    public static void CappedRecognitionThresholdStartsInNew()
    {
        var item = CappedRecognitionThreshold.NewCappedRecognitionThreshold("code-364", "title-193");
        Check.Equal(item.Status, CappedRecognitionThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRecognitionThresholdMovesToRunning()
    {
        var item = CappedRecognitionThreshold.NewCappedRecognitionThreshold("code-364", "title-193");
        try
        {
            item.MoveTo(CappedRecognitionThresholdStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRecognitionThresholdStatus.Running, "status");
    }

    public static void CappedRecognitionThresholdRejectsMovesFromFailed()
    {
        var item = CappedRecognitionThreshold.NewCappedRecognitionThreshold("code-364", "title-193");
        item.Status = CappedRecognitionThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedRecognitionThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedRecognitionThresholdStartsInNew();
        CappedRecognitionThresholdMovesToRunning();
        CappedRecognitionThresholdRejectsMovesFromFailed();
    }
}
