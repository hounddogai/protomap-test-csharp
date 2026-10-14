namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TrainingDueSlotChecks
{
    public static void TrainingDueSlotStartsInProposed()
    {
        var item = TrainingDueSlot.NewTrainingDueSlot("code-95", "label-241");
        Check.Equal(item.Status, TrainingDueSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueSlotMovesToApproved()
    {
        var item = TrainingDueSlot.NewTrainingDueSlot("code-95", "label-241");
        try
        {
            item.MoveTo(TrainingDueSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueSlotStatus.Approved, "status");
    }

    public static void TrainingDueSlotRejectsMovesFromWithdrawn()
    {
        var item = TrainingDueSlot.NewTrainingDueSlot("code-95", "label-241");
        item.Status = TrainingDueSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TrainingDueSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TrainingDueSlotStartsInProposed();
        TrainingDueSlotMovesToApproved();
        TrainingDueSlotRejectsMovesFromWithdrawn();
    }
}
