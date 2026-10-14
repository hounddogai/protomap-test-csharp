namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PrimaryActionPlanMappingChecks
{
    public static void PrimaryActionPlanMappingStartsInProposed()
    {
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
        Check.Equal(item.Status, PrimaryActionPlanMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryActionPlanMappingMovesToRejected()
    {
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
        try
        {
            item.MoveTo(PrimaryActionPlanMappingStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryActionPlanMappingStatus.Rejected, "status");
    }

    public static void PrimaryActionPlanMappingRejectsMovesFromWithdrawn()
    {
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
        item.Status = PrimaryActionPlanMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryActionPlanMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryActionPlanMappingStartsInProposed();
        PrimaryActionPlanMappingMovesToRejected();
        PrimaryActionPlanMappingRejectsMovesFromWithdrawn();
    }
}
