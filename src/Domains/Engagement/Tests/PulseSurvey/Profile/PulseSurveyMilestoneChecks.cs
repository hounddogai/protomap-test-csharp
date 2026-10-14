namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PulseSurveyMilestoneChecks
{
    public static void PulseSurveyMilestoneStartsInPlanned()
    {
        var item = PulseSurveyMilestone.NewPulseSurveyMilestone("code-332", "label-303");
        Check.Equal(item.Status, PulseSurveyMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyMilestoneMovesToCompleted()
    {
        var item = PulseSurveyMilestone.NewPulseSurveyMilestone("code-332", "label-303");
        try
        {
            item.MoveTo(PulseSurveyMilestoneStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyMilestoneStatus.Completed, "status");
    }

    public static void PulseSurveyMilestoneRejectsMovesFromCancelled()
    {
        var item = PulseSurveyMilestone.NewPulseSurveyMilestone("code-332", "label-303");
        item.Status = PulseSurveyMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PulseSurveyMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PulseSurveyMilestoneStartsInPlanned();
        PulseSurveyMilestoneMovesToCompleted();
        PulseSurveyMilestoneRejectsMovesFromCancelled();
    }
}
