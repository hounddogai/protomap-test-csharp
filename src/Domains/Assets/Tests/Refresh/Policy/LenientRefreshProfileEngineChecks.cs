namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientRefreshProfileEngineChecks
{
    public static void LenientRefreshProfileEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRefreshProfileEngine.DefaultLenientRefreshProfileEngine();
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRefreshProfileEngineKeepsResultsWithinBounds();
    }
}
