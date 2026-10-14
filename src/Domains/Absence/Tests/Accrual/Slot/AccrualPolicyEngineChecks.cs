namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualPolicyEngineChecks
{
    public static void AccrualPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualPolicyEngine.DefaultAccrualPolicyEngine(Flags.NewRegistry());
        var item = AccrualPolicy.NewAccrualPolicy("code-958", "label-944");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualPolicyEngineKeepsResultsWithinBounds();
    }
}
