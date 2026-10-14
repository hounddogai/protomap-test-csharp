namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierChecks
{
    public static void ReturnPlanTierStartsInCreated()
    {
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
        Check.Equal(item.Status, ReturnPlanTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanTierMovesToPublished()
    {
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
        try
        {
            item.MoveTo(ReturnPlanTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanTierStatus.Published, "status");
    }

    public static void ReturnPlanTierRejectsMovesFromRetired()
    {
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
        item.Status = ReturnPlanTierStatus.Retired;
        Check.Fails(() => item.MoveTo(ReturnPlanTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReturnPlanTierStartsInCreated();
        ReturnPlanTierMovesToPublished();
        ReturnPlanTierRejectsMovesFromRetired();
    }
}
