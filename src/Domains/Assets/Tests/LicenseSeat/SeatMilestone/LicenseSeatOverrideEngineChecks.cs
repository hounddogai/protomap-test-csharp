namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideEngineChecks
{
    public static void LicenseSeatOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatOverrideEngine.DefaultLicenseSeatOverrideEngine(Flags.NewRegistry());
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatOverrideEngineKeepsResultsWithinBounds();
    }
}
