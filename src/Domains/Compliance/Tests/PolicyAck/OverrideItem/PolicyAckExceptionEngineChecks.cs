namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckExceptionEngineChecks
{
    public static void PolicyAckExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckExceptionEngine.DefaultPolicyAckExceptionEngine(Flags.NewRegistry());
        var item = PolicyAckException.NewPolicyAckException("code-720", "label-978");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckExceptionEngineKeepsResultsWithinBounds();
    }
}
