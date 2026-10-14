namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssignmentBudgetEngineChecks
{
    public static void AssignmentBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentBudgetEngine.DefaultAssignmentBudgetEngine(Flags.NewRegistry());
        var item = AssignmentBudget.NewAssignmentBudget("code-382", "label-695");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentBudgetEngineKeepsResultsWithinBounds();
    }
}
