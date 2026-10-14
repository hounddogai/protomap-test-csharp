namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalRecognitionThresholdChecks
{
    public static void RegionalRecognitionThresholdStartsInPlanned()
    {
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
        Check.Equal(item.Status, RegionalRecognitionThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalRecognitionThresholdMovesToCompleted()
    {
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
        try
        {
            item.MoveTo(RegionalRecognitionThresholdStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalRecognitionThresholdStatus.Completed, "status");
    }

    public static void RegionalRecognitionThresholdRejectsMovesFromCancelled()
    {
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
        item.Status = RegionalRecognitionThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalRecognitionThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalRecognitionThresholdStartsInPlanned();
        RegionalRecognitionThresholdMovesToCompleted();
        RegionalRecognitionThresholdRejectsMovesFromCancelled();
    }
}
