namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorDraftEngineChecks
{
    public static void VisitorDraftEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorDraftEngine.DefaultVisitorDraftEngine(Flags.NewRegistry());
        var item = VisitorDraft.NewVisitorDraft("code-892", "title-877");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorDraftEngineKeepsResultsWithinBounds();
    }
}
