namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingLaptopClaimEngineChecks
{
    public static void RollingLaptopClaimEngineKeepsResultsWithinBounds()
    {
        var engine = RollingLaptopClaimEngine.DefaultRollingLaptopClaimEngine(Flags.NewRegistry());
        var item = RollingLaptopClaim.NewRollingLaptopClaim("code-455", "name-772");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingLaptopClaimEngineKeepsResultsWithinBounds();
    }
}
