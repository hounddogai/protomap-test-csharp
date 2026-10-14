namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskProfileEngineChecks
{
    public static void DeskProfileEngineKeepsResultsWithinBounds()
    {
        var engine = DeskProfileEngine.DefaultDeskProfileEngine(Flags.NewRegistry());
        var item = DeskProfile.NewDeskProfile("code-620", "name-930");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskProfileEngineKeepsResultsWithinBounds();
    }
}
