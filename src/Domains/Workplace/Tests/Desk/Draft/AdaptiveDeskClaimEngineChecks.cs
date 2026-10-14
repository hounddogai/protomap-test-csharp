namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AdaptiveDeskClaimEngineChecks
{
    public static void AdaptiveDeskClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveDeskClaimEngine.DefaultAdaptiveDeskClaimEngine(Flags.NewRegistry());
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("code-378", "label-486");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveDeskClaimEngineKeepsResultsWithinBounds();
    }
}
