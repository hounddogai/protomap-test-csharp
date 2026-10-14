namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedRecognitionVarianceChecks
{
    public static void WeightedRecognitionVarianceStartsInDraft()
    {
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("code-486", "label-607");
        Check.Equal(item.Status, WeightedRecognitionVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRecognitionVarianceMovesToActive()
    {
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("code-486", "label-607");
        try
        {
            item.MoveTo(WeightedRecognitionVarianceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRecognitionVarianceStatus.Active, "status");
    }

    public static void WeightedRecognitionVarianceRejectsMovesFromClosed()
    {
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("code-486", "label-607");
        item.Status = WeightedRecognitionVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedRecognitionVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedRecognitionVarianceStartsInDraft();
        WeightedRecognitionVarianceMovesToActive();
        WeightedRecognitionVarianceRejectsMovesFromClosed();
    }
}
