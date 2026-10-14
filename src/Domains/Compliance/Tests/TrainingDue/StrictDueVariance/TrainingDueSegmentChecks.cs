namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueSegmentChecks
{
    public static void TrainingDueSegmentStartsInProposed()
    {
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
        Check.Equal(item.Status, TrainingDueSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueSegmentMovesToRejected()
    {
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
        try
        {
            item.MoveTo(TrainingDueSegmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueSegmentStatus.Rejected, "status");
    }

    public static void TrainingDueSegmentRejectsMovesFromWithdrawn()
    {
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
        item.Status = TrainingDueSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TrainingDueSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TrainingDueSegmentStartsInProposed();
        TrainingDueSegmentMovesToRejected();
        TrainingDueSegmentRejectsMovesFromWithdrawn();
    }
}
