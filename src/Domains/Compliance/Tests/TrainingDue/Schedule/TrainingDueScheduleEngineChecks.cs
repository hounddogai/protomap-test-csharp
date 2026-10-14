namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueScheduleEngineChecks
{
    public static void TrainingDueScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueScheduleEngine.DefaultTrainingDueScheduleEngine(Flags.NewRegistry());
        var item = TrainingDueSchedule.NewTrainingDueSchedule("code-375", "title-726");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueScheduleEngineKeepsResultsWithinBounds();
    }
}
