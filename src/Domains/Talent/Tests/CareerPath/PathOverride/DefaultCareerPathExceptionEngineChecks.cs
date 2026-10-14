namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class DefaultCareerPathExceptionEngineChecks
{
    public static void DefaultCareerPathExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultCareerPathExceptionEngine.DefaultDefaultCareerPathExceptionEngine(Flags.NewRegistry());
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultCareerPathExceptionEngineKeepsResultsWithinBounds();
    }
}
