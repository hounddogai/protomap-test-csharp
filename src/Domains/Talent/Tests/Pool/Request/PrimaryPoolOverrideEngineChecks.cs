namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PrimaryPoolOverrideEngineChecks
{
    public static void PrimaryPoolOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPoolOverrideEngine.DefaultPrimaryPoolOverrideEngine();
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPoolOverrideEngineKeepsResultsWithinBounds();
    }
}
