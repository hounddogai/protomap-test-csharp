namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunPreferenceEngineChecks
{
    public static void PayRunPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunPreferenceEngine.DefaultPayRunPreferenceEngine(Flags.NewRegistry());
        var item = PayRunPreference.NewPayRunPreference("code-50", "title-435");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunPreferenceEngineKeepsResultsWithinBounds();
    }
}
