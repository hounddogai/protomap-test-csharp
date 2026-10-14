namespace Acme.Workforce.Compliance.Tests.TrainingDue.Tier;

using Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientTrainingDueTierEngineChecks
{
    public static void LenientTrainingDueTierEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTrainingDueTierEngine.DefaultLenientTrainingDueTierEngine();
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("code-213", "title-486");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTrainingDueTierEngineKeepsResultsWithinBounds();
    }
}
