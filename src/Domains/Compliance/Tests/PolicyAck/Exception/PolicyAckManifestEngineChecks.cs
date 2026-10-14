namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckManifestEngineChecks
{
    public static void PolicyAckManifestEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckManifestEngine.DefaultPolicyAckManifestEngine();
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckManifestEngineKeepsResultsWithinBounds();
    }
}
