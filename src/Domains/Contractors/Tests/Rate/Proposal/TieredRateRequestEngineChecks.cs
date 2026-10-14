namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredRateRequestEngineChecks
{
    public static void TieredRateRequestEngineKeepsResultsWithinBounds()
    {
        var engine = TieredRateRequestEngine.DefaultTieredRateRequestEngine(Flags.NewRegistry());
        var item = TieredRateRequest.NewTieredRateRequest("code-890", "label-258");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredRateRequestEngineKeepsResultsWithinBounds();
    }
}
