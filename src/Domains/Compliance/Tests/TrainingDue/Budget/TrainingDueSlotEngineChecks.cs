namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TrainingDueSlotEngineChecks
{
    public static void TrainingDueSlotEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueSlotEngine.DefaultTrainingDueSlotEngine(Flags.NewRegistry());
        var item = TrainingDueSlot.NewTrainingDueSlot("code-95", "label-241");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueSlotEngineKeepsResultsWithinBounds();
    }
}
