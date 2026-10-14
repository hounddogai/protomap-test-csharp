namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacyPulseSurveyCheckpointChecks
{
    public static void LegacyPulseSurveyCheckpointStartsInProposed()
    {
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("code-583", "label-71");
        Check.Equal(item.Status, LegacyPulseSurveyCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPulseSurveyCheckpointMovesToRejected()
    {
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("code-583", "label-71");
        try
        {
            item.MoveTo(LegacyPulseSurveyCheckpointStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPulseSurveyCheckpointStatus.Rejected, "status");
    }

    public static void LegacyPulseSurveyCheckpointRejectsMovesFromWithdrawn()
    {
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("code-583", "label-71");
        item.Status = LegacyPulseSurveyCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyPulseSurveyCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyPulseSurveyCheckpointStartsInProposed();
        LegacyPulseSurveyCheckpointMovesToRejected();
        LegacyPulseSurveyCheckpointRejectsMovesFromWithdrawn();
    }
}
