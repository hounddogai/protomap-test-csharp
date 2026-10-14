namespace Acme.Workforce.Workplace.Tests.Desk.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingDeskRollupEngineChecks
{
    public static void PendingDeskRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PendingDeskRollupEngine.DefaultPendingDeskRollupEngine(Flags.NewRegistry());
        var item = PendingDeskRollup.NewPendingDeskRollup("code-767", "name-618");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingDeskRollupEngineKeepsResultsWithinBounds();
    }
}
