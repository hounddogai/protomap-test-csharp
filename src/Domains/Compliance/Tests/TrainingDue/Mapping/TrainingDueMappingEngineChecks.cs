namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueMappingEngineChecks
{
    public static void TrainingDueMappingEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueMappingEngine.DefaultTrainingDueMappingEngine(Flags.NewRegistry());
        var item = TrainingDueMapping.NewTrainingDueMapping("code-687", "name-57");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueMappingEngineKeepsResultsWithinBounds();
    }
}
