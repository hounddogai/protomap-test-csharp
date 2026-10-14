namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleLedgerEngineChecks
{
    public static void MeritCycleLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleLedgerEngine.DefaultMeritCycleLedgerEngine(Flags.NewRegistry());
        var item = MeritCycleLedger.NewMeritCycleLedger("code-167", "label-616");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleLedgerEngineKeepsResultsWithinBounds();
    }
}
