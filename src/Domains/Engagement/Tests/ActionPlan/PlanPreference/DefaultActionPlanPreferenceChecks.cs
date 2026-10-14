namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class DefaultActionPlanPreferenceChecks
{
    public static void DefaultActionPlanPreferenceStartsInOpen()
    {
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("code-206", "name-62");
        Check.Equal(item.Status, DefaultActionPlanPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultActionPlanPreferenceMovesToInReview()
    {
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("code-206", "name-62");
        try
        {
            item.MoveTo(DefaultActionPlanPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultActionPlanPreferenceStatus.InReview, "status");
    }

    public static void DefaultActionPlanPreferenceRejectsMovesFromArchived()
    {
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("code-206", "name-62");
        item.Status = DefaultActionPlanPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultActionPlanPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultActionPlanPreferenceStartsInOpen();
        DefaultActionPlanPreferenceMovesToInReview();
        DefaultActionPlanPreferenceRejectsMovesFromArchived();
    }
}
