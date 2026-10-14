namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueGroupEngineChecks
{
    public static void TrainingDueGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueGroupEngine.DefaultTrainingDueGroupEngine(Flags.NewRegistry());
        var item = TrainingDueGroup.NewTrainingDueGroup("code-784", "title-37");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueGroupEngineKeepsResultsWithinBounds();
    }
}
