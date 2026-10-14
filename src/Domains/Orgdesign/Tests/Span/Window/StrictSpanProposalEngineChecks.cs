namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StrictSpanProposalEngineChecks
{
    public static void StrictSpanProposalEngineKeepsResultsWithinBounds()
    {
        var engine = StrictSpanProposalEngine.DefaultStrictSpanProposalEngine(Flags.NewRegistry());
        var item = StrictSpanProposal.NewStrictSpanProposal("code-465", "title-392");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictSpanProposalEngineKeepsResultsWithinBounds();
    }
}
