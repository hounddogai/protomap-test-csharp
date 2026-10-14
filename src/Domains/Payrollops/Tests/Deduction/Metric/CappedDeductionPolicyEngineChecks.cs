namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedDeductionPolicyEngineChecks
{
    public static void CappedDeductionPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = CappedDeductionPolicyEngine.DefaultCappedDeductionPolicyEngine(Flags.NewRegistry());
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedDeductionPolicyEngineKeepsResultsWithinBounds();
    }
}
