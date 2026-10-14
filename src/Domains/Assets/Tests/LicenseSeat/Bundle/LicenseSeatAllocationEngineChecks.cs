namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationEngineChecks
{
    public static void LicenseSeatAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatAllocationEngine.DefaultLicenseSeatAllocationEngine(Flags.NewRegistry());
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatAllocationEngineKeepsResultsWithinBounds();
    }
}
