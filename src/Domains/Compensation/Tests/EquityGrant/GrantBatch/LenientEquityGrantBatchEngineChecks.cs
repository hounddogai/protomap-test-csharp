namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class LenientEquityGrantBatchEngineChecks
{
    public static void LenientEquityGrantBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LenientEquityGrantBatchEngine.DefaultLenientEquityGrantBatchEngine(Flags.NewRegistry());
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("code-59", "name-319");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientEquityGrantBatchEngineKeepsResultsWithinBounds();
    }
}
