namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RecognitionMetricChecks
{
    public static void RecognitionMetricStartsInDraft()
    {
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
        Check.Equal(item.Status, RecognitionMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionMetricMovesToClosed()
    {
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
        try
        {
            item.MoveTo(RecognitionMetricStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionMetricStatus.Closed, "status");
    }

    public static void RecognitionMetricRejectsMovesFromClosed()
    {
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
        item.Status = RecognitionMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionMetricStartsInDraft();
        RecognitionMetricMovesToClosed();
        RecognitionMetricRejectsMovesFromClosed();
    }
}
