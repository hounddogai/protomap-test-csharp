namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideEngineChecks
{
    public static void TieredPolicyAckOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPolicyAckOverrideEngine.DefaultTieredPolicyAckOverrideEngine(Flags.NewRegistry());
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPolicyAckOverrideEngineKeepsResultsWithinBounds();
    }
}
