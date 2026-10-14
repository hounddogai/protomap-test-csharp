namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowEngineChecks
{
    public static void AccrualWindowEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualWindowEngine.DefaultAccrualWindowEngine(Flags.NewRegistry());
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualWindowEngineKeepsResultsWithinBounds();
    }
}
