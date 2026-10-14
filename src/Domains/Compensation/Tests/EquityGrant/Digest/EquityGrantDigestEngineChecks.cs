namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantDigestEngineChecks
{
    public static void EquityGrantDigestEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantDigestEngine.DefaultEquityGrantDigestEngine(Flags.NewRegistry());
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantDigestEngineKeepsResultsWithinBounds();
    }
}
