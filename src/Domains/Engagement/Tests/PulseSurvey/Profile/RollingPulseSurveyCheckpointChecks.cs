namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingPulseSurveyCheckpointChecks
{
    public static void RollingPulseSurveyCheckpointStartsInDraft()
    {
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("code-69", "title-188");
        Check.Equal(item.Status, RollingPulseSurveyCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPulseSurveyCheckpointMovesToSuspended()
    {
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("code-69", "title-188");
        try
        {
            item.MoveTo(RollingPulseSurveyCheckpointStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPulseSurveyCheckpointStatus.Suspended, "status");
    }

    public static void RollingPulseSurveyCheckpointRejectsMovesFromClosed()
    {
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("code-69", "title-188");
        item.Status = RollingPulseSurveyCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingPulseSurveyCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingPulseSurveyCheckpointStartsInDraft();
        RollingPulseSurveyCheckpointMovesToSuspended();
        RollingPulseSurveyCheckpointRejectsMovesFromClosed();
    }
}
