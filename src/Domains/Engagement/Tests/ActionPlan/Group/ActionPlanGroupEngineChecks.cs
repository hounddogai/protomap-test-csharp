namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ActionPlanGroupEngineChecks
{
    public static void ActionPlanGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanGroupEngine.DefaultActionPlanGroupEngine(Flags.NewRegistry());
        var item = ActionPlanGroup.NewActionPlanGroup("code-415", "label-523");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanGroupEngineKeepsResultsWithinBounds();
    }
}
