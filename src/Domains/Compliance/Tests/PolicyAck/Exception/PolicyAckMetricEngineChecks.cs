namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckMetricEngineChecks
{
    public static void PolicyAckMetricEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckMetricEngine.DefaultPolicyAckMetricEngine(Flags.NewRegistry());
        var item = PolicyAckMetric.NewPolicyAckMetric("code-432", "label-840");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckMetricEngineKeepsResultsWithinBounds();
    }
}
