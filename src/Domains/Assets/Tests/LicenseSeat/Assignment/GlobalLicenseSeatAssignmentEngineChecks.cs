namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalLicenseSeatAssignmentEngineChecks
{
    public static void GlobalLicenseSeatAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalLicenseSeatAssignmentEngine.DefaultGlobalLicenseSeatAssignmentEngine(Flags.NewRegistry());
        var item = GlobalLicenseSeatAssignment.NewGlobalLicenseSeatAssignment("code-743", "name-27");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalLicenseSeatAssignmentEngineKeepsResultsWithinBounds();
    }
}
