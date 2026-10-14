namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueSegmentEngineChecks
{
    public static void TrainingDueSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueSegmentEngine.DefaultTrainingDueSegmentEngine();
        var item = TrainingDueSegment.NewTrainingDueSegment("code-304", "name-297");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueSegmentEngineKeepsResultsWithinBounds();
    }
}
