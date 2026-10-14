namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingVisitorDraftEngineChecks
{
    public static void RollingVisitorDraftEngineKeepsResultsWithinBounds()
    {
        var engine = RollingVisitorDraftEngine.DefaultRollingVisitorDraftEngine(Flags.NewRegistry());
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingVisitorDraftEngineKeepsResultsWithinBounds();
    }
}
