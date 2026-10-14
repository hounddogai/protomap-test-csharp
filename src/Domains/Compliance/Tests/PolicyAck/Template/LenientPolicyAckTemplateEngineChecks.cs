namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientPolicyAckTemplateEngineChecks
{
    public static void LenientPolicyAckTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPolicyAckTemplateEngine.DefaultLenientPolicyAckTemplateEngine(Flags.NewRegistry());
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("code-40", "title-134");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPolicyAckTemplateEngineKeepsResultsWithinBounds();
    }
}
