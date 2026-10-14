namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleEngineChecks
{
    public static void DefaultVisitorRuleEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultVisitorRuleEngine.DefaultDefaultVisitorRuleEngine(Flags.NewRegistry());
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultVisitorRuleEngineKeepsResultsWithinBounds();
    }
}
