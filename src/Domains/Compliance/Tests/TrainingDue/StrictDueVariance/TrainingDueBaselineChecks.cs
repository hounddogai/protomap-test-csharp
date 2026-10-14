namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueBaselineChecks
{
    public static void TrainingDueBaselineStartsInProposed()
    {
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
        Check.Equal(item.Status, TrainingDueBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueBaselineMovesToRejected()
    {
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
        try
        {
            item.MoveTo(TrainingDueBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueBaselineStatus.Rejected, "status");
    }

    public static void TrainingDueBaselineRejectsMovesFromWithdrawn()
    {
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
        item.Status = TrainingDueBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TrainingDueBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TrainingDueBaselineStartsInProposed();
        TrainingDueBaselineMovesToRejected();
        TrainingDueBaselineRejectsMovesFromWithdrawn();
    }
}
