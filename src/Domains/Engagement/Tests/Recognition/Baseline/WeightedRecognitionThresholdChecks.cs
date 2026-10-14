namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedRecognitionThresholdChecks
{
    public static void WeightedRecognitionThresholdStartsInOpen()
    {
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("code-809", "label-43");
        Check.Equal(item.Status, WeightedRecognitionThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRecognitionThresholdMovesToResolved()
    {
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("code-809", "label-43");
        try
        {
            item.MoveTo(WeightedRecognitionThresholdStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRecognitionThresholdStatus.Resolved, "status");
    }

    public static void WeightedRecognitionThresholdRejectsMovesFromArchived()
    {
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("code-809", "label-43");
        item.Status = WeightedRecognitionThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedRecognitionThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedRecognitionThresholdStartsInOpen();
        WeightedRecognitionThresholdMovesToResolved();
        WeightedRecognitionThresholdRejectsMovesFromArchived();
    }
}
