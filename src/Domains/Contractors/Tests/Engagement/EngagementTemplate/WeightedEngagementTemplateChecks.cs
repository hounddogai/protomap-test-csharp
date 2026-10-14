namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class WeightedEngagementTemplateChecks
{
    public static void WeightedEngagementTemplateStartsInRequested()
    {
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
        Check.Equal(item.Status, WeightedEngagementTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedEngagementTemplateMovesToGranted()
    {
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
        try
        {
            item.MoveTo(WeightedEngagementTemplateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedEngagementTemplateStatus.Granted, "status");
    }

    public static void WeightedEngagementTemplateRejectsMovesFromExpired()
    {
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
        item.Status = WeightedEngagementTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedEngagementTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedEngagementTemplateStartsInRequested();
        WeightedEngagementTemplateMovesToGranted();
        WeightedEngagementTemplateRejectsMovesFromExpired();
    }
}
