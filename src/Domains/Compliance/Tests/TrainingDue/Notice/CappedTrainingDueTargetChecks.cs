namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedTrainingDueTargetChecks
{
    public static void CappedTrainingDueTargetStartsInIdle()
    {
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("code-545", "label-814");
        Check.Equal(item.Status, CappedTrainingDueTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTrainingDueTargetMovesToReady()
    {
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("code-545", "label-814");
        try
        {
            item.MoveTo(CappedTrainingDueTargetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTrainingDueTargetStatus.Ready, "status");
    }

    public static void CappedTrainingDueTargetRejectsMovesFromStopped()
    {
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("code-545", "label-814");
        item.Status = CappedTrainingDueTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedTrainingDueTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedTrainingDueTargetStartsInIdle();
        CappedTrainingDueTargetMovesToReady();
        CappedTrainingDueTargetRejectsMovesFromStopped();
    }
}
