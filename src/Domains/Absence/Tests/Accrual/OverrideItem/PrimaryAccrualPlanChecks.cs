namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PrimaryAccrualPlanChecks
{
    public static void PrimaryAccrualPlanStartsInCreated()
    {
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("code-827", "label-185");
        Check.Equal(item.Status, PrimaryAccrualPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAccrualPlanMovesToRetired()
    {
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("code-827", "label-185");
        try
        {
            item.MoveTo(PrimaryAccrualPlanStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAccrualPlanStatus.Retired, "status");
    }

    public static void PrimaryAccrualPlanRejectsMovesFromRetired()
    {
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("code-827", "label-185");
        item.Status = PrimaryAccrualPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryAccrualPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryAccrualPlanStartsInCreated();
        PrimaryAccrualPlanMovesToRetired();
        PrimaryAccrualPlanRejectsMovesFromRetired();
    }
}
