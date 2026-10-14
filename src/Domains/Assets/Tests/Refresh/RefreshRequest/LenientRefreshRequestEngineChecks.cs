namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class LenientRefreshRequestEngineChecks
{
    public static void LenientRefreshRequestEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRefreshRequestEngine.DefaultLenientRefreshRequestEngine(Flags.NewRegistry());
        var item = LenientRefreshRequest.NewLenientRefreshRequest("code-174", "title-23");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRefreshRequestEngineKeepsResultsWithinBounds();
    }
}
