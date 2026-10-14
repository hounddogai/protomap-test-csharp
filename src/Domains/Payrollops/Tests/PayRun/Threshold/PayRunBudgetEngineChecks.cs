namespace Acme.Workforce.Payrollops.Tests.PayRun.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayRunBudgetEngineChecks
{
    public static void PayRunBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunBudgetEngine.DefaultPayRunBudgetEngine(Flags.NewRegistry());
        var item = PayRunBudget.NewPayRunBudget("code-742", "name-701");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunBudgetEngineKeepsResultsWithinBounds();
    }
}
