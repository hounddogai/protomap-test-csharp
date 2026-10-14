namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PulseSurveySnapshotChecks
{
    public static void PulseSurveySnapshotStartsInIdle()
    {
        var item = PulseSurveySnapshot.NewPulseSurveySnapshot("code-221", "title-388");
        Check.Equal(item.Status, PulseSurveySnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveySnapshotMovesToWarming()
    {
        var item = PulseSurveySnapshot.NewPulseSurveySnapshot("code-221", "title-388");
        try
        {
            item.MoveTo(PulseSurveySnapshotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveySnapshotStatus.Warming, "status");
    }

    public static void PulseSurveySnapshotRejectsMovesFromStopped()
    {
        var item = PulseSurveySnapshot.NewPulseSurveySnapshot("code-221", "title-388");
        item.Status = PulseSurveySnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveySnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveySnapshotStartsInIdle();
        PulseSurveySnapshotMovesToWarming();
        PulseSurveySnapshotRejectsMovesFromStopped();
    }
}
