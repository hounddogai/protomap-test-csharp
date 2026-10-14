namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueMilestoneEngineChecks
{
    public static void TrainingDueMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueMilestoneEngine.DefaultTrainingDueMilestoneEngine(Flags.NewRegistry());
        var item = TrainingDueMilestone.NewTrainingDueMilestone("code-138", "label-206");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueMilestoneEngineKeepsResultsWithinBounds();
    }
}
