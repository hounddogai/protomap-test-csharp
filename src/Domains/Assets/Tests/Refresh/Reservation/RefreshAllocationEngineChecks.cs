namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RefreshAllocationEngineChecks
{
    public static void RefreshAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshAllocationEngine.DefaultRefreshAllocationEngine(Flags.NewRegistry());
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshAllocationEngineKeepsResultsWithinBounds();
    }
}
