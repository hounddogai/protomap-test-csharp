namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class LenientTrainingDueScheduleEngineChecks
{
    public static void LenientTrainingDueScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTrainingDueScheduleEngine.DefaultLenientTrainingDueScheduleEngine();
        var item = LenientTrainingDueSchedule.NewLenientTrainingDueSchedule("code-648", "label-466");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTrainingDueScheduleEngineKeepsResultsWithinBounds();
    }
}
