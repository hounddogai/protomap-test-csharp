namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingAccrualWindowEngineChecks
{
    public static void PendingAccrualWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PendingAccrualWindowEngine.DefaultPendingAccrualWindowEngine(Flags.NewRegistry());
        var item = PendingAccrualWindow.NewPendingAccrualWindow("code-930", "title-473");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingAccrualWindowEngineKeepsResultsWithinBounds();
    }
}
