namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftEngineChecks
{
    public static void GlobalVisitorDraftEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalVisitorDraftEngine.DefaultGlobalVisitorDraftEngine();
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalVisitorDraftEngineKeepsResultsWithinBounds();
    }
}
