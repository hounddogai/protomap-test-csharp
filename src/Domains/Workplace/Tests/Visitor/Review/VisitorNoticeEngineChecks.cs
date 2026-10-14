namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorNoticeEngineChecks
{
    public static void VisitorNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorNoticeEngine.DefaultVisitorNoticeEngine(Flags.NewRegistry());
        var item = VisitorNotice.NewVisitorNotice("code-770", "name-864");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorNoticeEngineKeepsResultsWithinBounds();
    }
}
