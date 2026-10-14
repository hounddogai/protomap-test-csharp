namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedAccrualGroupChecks
{
    public static void WeightedAccrualGroupStartsInPlanned()
    {
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("code-167", "title-153");
        Check.Equal(item.Status, WeightedAccrualGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccrualGroupMovesToCompleted()
    {
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("code-167", "title-153");
        try
        {
            item.MoveTo(WeightedAccrualGroupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccrualGroupStatus.Completed, "status");
    }

    public static void WeightedAccrualGroupRejectsMovesFromCancelled()
    {
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("code-167", "title-153");
        item.Status = WeightedAccrualGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedAccrualGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedAccrualGroupStartsInPlanned();
        WeightedAccrualGroupMovesToCompleted();
        WeightedAccrualGroupRejectsMovesFromCancelled();
    }
}
