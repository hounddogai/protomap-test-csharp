namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class DefaultMobilityTargetEngineChecks
{
    public static void DefaultMobilityTargetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultMobilityTargetEngine.DefaultDefaultMobilityTargetEngine();
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("code-498", "title-646");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultMobilityTargetEngineKeepsResultsWithinBounds();
    }
}
