namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryTrainingDuePlanEngineChecks
{
    public static void PrimaryTrainingDuePlanEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryTrainingDuePlanEngine.DefaultPrimaryTrainingDuePlanEngine();
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("code-861", "title-972");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryTrainingDuePlanEngineKeepsResultsWithinBounds();
    }
}
