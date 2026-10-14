namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualPlanChecks
{
    public static void AccrualPlanStartsInCreated()
    {
        var item = AccrualPlan.NewAccrualPlan("code-138", "title-827");
        Check.Equal(item.Status, AccrualPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualPlanMovesToPublished()
    {
        var item = AccrualPlan.NewAccrualPlan("code-138", "title-827");
        try
        {
            item.MoveTo(AccrualPlanStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualPlanStatus.Published, "status");
    }

    public static void AccrualPlanRejectsMovesFromRetired()
    {
        var item = AccrualPlan.NewAccrualPlan("code-138", "title-827");
        item.Status = AccrualPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(AccrualPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccrualPlanStartsInCreated();
        AccrualPlanMovesToPublished();
        AccrualPlanRejectsMovesFromRetired();
    }
}
