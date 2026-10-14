namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackTrainingDueSegmentChecks
{
    public static void FallbackTrainingDueSegmentStartsInIdle()
    {
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("code-589", "label-410");
        Check.Equal(item.Status, FallbackTrainingDueSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackTrainingDueSegmentMovesToDraining()
    {
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("code-589", "label-410");
        try
        {
            item.MoveTo(FallbackTrainingDueSegmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackTrainingDueSegmentStatus.Draining, "status");
    }

    public static void FallbackTrainingDueSegmentRejectsMovesFromStopped()
    {
        var item = FallbackTrainingDueSegment.NewFallbackTrainingDueSegment("code-589", "label-410");
        item.Status = FallbackTrainingDueSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackTrainingDueSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackTrainingDueSegmentStartsInIdle();
        FallbackTrainingDueSegmentMovesToDraining();
        FallbackTrainingDueSegmentRejectsMovesFromStopped();
    }
}
