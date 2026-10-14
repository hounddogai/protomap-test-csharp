namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualTargetChecks
{
    public static void AccrualTargetStartsInNew()
    {
        var item = AccrualTarget.NewAccrualTarget("code-866", "name-940");
        Check.Equal(item.Status, AccrualTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualTargetMovesToFailed()
    {
        var item = AccrualTarget.NewAccrualTarget("code-866", "name-940");
        try
        {
            item.MoveTo(AccrualTargetStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualTargetStatus.Failed, "status");
    }

    public static void AccrualTargetRejectsMovesFromFailed()
    {
        var item = AccrualTarget.NewAccrualTarget("code-866", "name-940");
        item.Status = AccrualTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(AccrualTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        AccrualTargetStartsInNew();
        AccrualTargetMovesToFailed();
        AccrualTargetRejectsMovesFromFailed();
    }
}
