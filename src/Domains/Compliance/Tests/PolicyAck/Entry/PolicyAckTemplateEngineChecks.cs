namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckTemplateEngineChecks
{
    public static void PolicyAckTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckTemplateEngine.DefaultPolicyAckTemplateEngine();
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckTemplateEngineKeepsResultsWithinBounds();
    }
}
