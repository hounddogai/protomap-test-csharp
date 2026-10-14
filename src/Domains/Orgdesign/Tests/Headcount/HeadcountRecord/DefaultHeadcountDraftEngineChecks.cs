namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class DefaultHeadcountDraftEngineChecks
{
    public static void DefaultHeadcountDraftEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultHeadcountDraftEngine.DefaultDefaultHeadcountDraftEngine();
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("code-35", "label-540");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHeadcountDraftEngineKeepsResultsWithinBounds();
    }
}
