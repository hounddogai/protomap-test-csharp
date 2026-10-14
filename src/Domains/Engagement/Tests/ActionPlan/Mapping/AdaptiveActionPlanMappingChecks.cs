namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMappingChecks
{
    public static void AdaptiveActionPlanMappingStartsInOpen()
    {
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("code-575", "title-100");
        Check.Equal(item.Status, AdaptiveActionPlanMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveActionPlanMappingMovesToArchived()
    {
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("code-575", "title-100");
        try
        {
            item.MoveTo(AdaptiveActionPlanMappingStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveActionPlanMappingStatus.Archived, "status");
    }

    public static void AdaptiveActionPlanMappingRejectsMovesFromArchived()
    {
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("code-575", "title-100");
        item.Status = AdaptiveActionPlanMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveActionPlanMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMappingStartsInOpen();
        AdaptiveActionPlanMappingMovesToArchived();
        AdaptiveActionPlanMappingRejectsMovesFromArchived();
    }
}
