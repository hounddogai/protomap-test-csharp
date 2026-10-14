namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueRollupEngineChecks
{
    public static void TrainingDueRollupEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueRollupEngine.DefaultTrainingDueRollupEngine(Flags.NewRegistry());
        var item = TrainingDueRollup.NewTrainingDueRollup("code-985", "label-559");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueRollupEngineKeepsResultsWithinBounds();
    }
}
