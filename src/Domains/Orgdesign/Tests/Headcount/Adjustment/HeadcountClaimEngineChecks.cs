namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountClaimEngineChecks
{
    public static void HeadcountClaimEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountClaimEngine.DefaultHeadcountClaimEngine(Flags.NewRegistry());
        var item = HeadcountClaim.NewHeadcountClaim("code-248", "title-690");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountClaimEngineKeepsResultsWithinBounds();
    }
}
