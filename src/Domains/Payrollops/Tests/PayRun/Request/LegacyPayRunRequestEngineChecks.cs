namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyPayRunRequestEngineChecks
{
    public static void LegacyPayRunRequestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayRunRequestEngine.DefaultLegacyPayRunRequestEngine(Flags.NewRegistry());
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayRunRequestEngineKeepsResultsWithinBounds();
    }
}
