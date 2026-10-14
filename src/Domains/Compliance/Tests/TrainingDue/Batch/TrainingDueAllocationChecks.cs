namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueAllocationChecks
{
    public static void TrainingDueAllocationStartsInNew()
    {
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
        Check.Equal(item.Status, TrainingDueAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueAllocationMovesToQueued()
    {
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
        try
        {
            item.MoveTo(TrainingDueAllocationStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueAllocationStatus.Queued, "status");
    }

    public static void TrainingDueAllocationRejectsMovesFromFailed()
    {
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
        item.Status = TrainingDueAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(TrainingDueAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        TrainingDueAllocationStartsInNew();
        TrainingDueAllocationMovesToQueued();
        TrainingDueAllocationRejectsMovesFromFailed();
    }
}
