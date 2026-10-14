namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentRuleEngineChecks
{
    public static void AssignmentRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentRuleEngine.DefaultAssignmentRuleEngine(Flags.NewRegistry());
        var item = AssignmentRule.NewAssignmentRule("code-37", "title-585");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentRuleEngineKeepsResultsWithinBounds();
    }
}
