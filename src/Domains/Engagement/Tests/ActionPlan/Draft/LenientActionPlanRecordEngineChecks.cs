namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientActionPlanRecordEngineChecks
{
    public static void LenientActionPlanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LenientActionPlanRecordEngine.DefaultLenientActionPlanRecordEngine();
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("code-509", "name-170");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientActionPlanRecordEngineKeepsResultsWithinBounds();
    }
}
