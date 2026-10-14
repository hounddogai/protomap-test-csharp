namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanRecordEngineChecks
{
    public static void ActionPlanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanRecordEngine.DefaultActionPlanRecordEngine(Flags.NewRegistry());
        var item = ActionPlanRecord.NewActionPlanRecord("code-41", "name-279");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanRecordEngineKeepsResultsWithinBounds();
    }
}
