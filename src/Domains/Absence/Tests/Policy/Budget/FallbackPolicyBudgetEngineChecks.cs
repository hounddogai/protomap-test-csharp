namespace Acme.Workforce.Absence.Tests.Policy.Budget;

using Acme.Workforce.Absence.Policy.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackPolicyBudgetEngineChecks
{
    public static void FallbackPolicyBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPolicyBudgetEngine.DefaultFallbackPolicyBudgetEngine(Flags.NewRegistry());
        var item = FallbackPolicyBudget.NewFallbackPolicyBudget("code-114", "name-381");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPolicyBudgetEngineKeepsResultsWithinBounds();
    }
}
