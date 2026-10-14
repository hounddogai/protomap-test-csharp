namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueBundleEngineChecks
{
    public static void TrainingDueBundleEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueBundleEngine.DefaultTrainingDueBundleEngine();
        var item = TrainingDueBundle.NewTrainingDueBundle("code-975", "title-831");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueBundleEngineKeepsResultsWithinBounds();
    }
}
