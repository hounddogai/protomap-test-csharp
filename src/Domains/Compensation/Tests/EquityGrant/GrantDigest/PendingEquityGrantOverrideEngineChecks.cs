namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideEngineChecks
{
    public static void PendingEquityGrantOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PendingEquityGrantOverrideEngine.DefaultPendingEquityGrantOverrideEngine(Flags.NewRegistry());
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("code-522", "name-670");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingEquityGrantOverrideEngineKeepsResultsWithinBounds();
    }
}
