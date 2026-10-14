namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PulseSurveySnapshotEngineChecks
{
    public static void PulseSurveySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveySnapshotEngine.DefaultPulseSurveySnapshotEngine();
        var item = PulseSurveySnapshot.NewPulseSurveySnapshot("code-221", "title-388");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveySnapshotEngineKeepsResultsWithinBounds();
    }
}
