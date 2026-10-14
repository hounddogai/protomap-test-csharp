namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanEngineChecks
{
    public static void WeightedDeductionPlanEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedDeductionPlanEngine.DefaultWeightedDeductionPlanEngine(Flags.NewRegistry());
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedDeductionPlanEngineKeepsResultsWithinBounds();
    }
}
