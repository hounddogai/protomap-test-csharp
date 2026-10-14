namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueBaselineEngineChecks
{
    public static void TrainingDueBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueBaselineEngine.DefaultTrainingDueBaselineEngine();
        var item = TrainingDueBaseline.NewTrainingDueBaseline("code-558", "label-587");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueBaselineEngineKeepsResultsWithinBounds();
    }
}
