namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorPolicyEngineChecks
{
    public static void VisitorPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorPolicyEngine.DefaultVisitorPolicyEngine(Flags.NewRegistry());
        var item = VisitorPolicy.NewVisitorPolicy("code-224", "name-728");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorPolicyEngineKeepsResultsWithinBounds();
    }
}
