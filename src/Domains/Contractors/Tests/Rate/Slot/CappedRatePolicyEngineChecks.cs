namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedRatePolicyEngineChecks
{
    public static void CappedRatePolicyEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRatePolicyEngine.DefaultCappedRatePolicyEngine(Flags.NewRegistry());
        var item = CappedRatePolicy.NewCappedRatePolicy("code-791", "title-783");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRatePolicyEngineKeepsResultsWithinBounds();
    }
}
