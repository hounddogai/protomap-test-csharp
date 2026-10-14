namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestChecks
{
    public static void ManualPulseSurveyDigestStartsInIdle()
    {
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
        Check.Equal(item.Status, ManualPulseSurveyDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveyDigestMovesToDraining()
    {
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
        try
        {
            item.MoveTo(ManualPulseSurveyDigestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveyDigestStatus.Draining, "status");
    }

    public static void ManualPulseSurveyDigestRejectsMovesFromStopped()
    {
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
        item.Status = ManualPulseSurveyDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualPulseSurveyDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveyDigestStartsInIdle();
        ManualPulseSurveyDigestMovesToDraining();
        ManualPulseSurveyDigestRejectsMovesFromStopped();
    }
}
