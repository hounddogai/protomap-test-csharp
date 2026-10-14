namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PrimaryMeritCycleWindowEngineChecks
{
    public static void PrimaryMeritCycleWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryMeritCycleWindowEngine.DefaultPrimaryMeritCycleWindowEngine(Flags.NewRegistry());
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("code-880", "name-186");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleWindowEngineKeepsResultsWithinBounds();
    }
}
