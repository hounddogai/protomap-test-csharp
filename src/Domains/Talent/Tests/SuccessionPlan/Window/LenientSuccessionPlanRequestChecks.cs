namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRequestChecks
{
    public static void LenientSuccessionPlanRequestStartsInProposed()
    {
        var item = LenientSuccessionPlanRequest.NewLenientSuccessionPlanRequest("code-692", "title-679");
        Check.Equal(item.Status, LenientSuccessionPlanRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanRequestMovesToApproved()
    {
        var item = LenientSuccessionPlanRequest.NewLenientSuccessionPlanRequest("code-692", "title-679");
        try
        {
            item.MoveTo(LenientSuccessionPlanRequestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanRequestStatus.Approved, "status");
    }

    public static void LenientSuccessionPlanRequestRejectsMovesFromWithdrawn()
    {
        var item = LenientSuccessionPlanRequest.NewLenientSuccessionPlanRequest("code-692", "title-679");
        item.Status = LenientSuccessionPlanRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanRequestStartsInProposed();
        LenientSuccessionPlanRequestMovesToApproved();
        LenientSuccessionPlanRequestRejectsMovesFromWithdrawn();
    }
}
