namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatQuotaEngineChecks
{
    public static void LicenseSeatQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatQuotaEngine.DefaultLicenseSeatQuotaEngine(Flags.NewRegistry());
        var item = LicenseSeatQuota.NewLicenseSeatQuota("code-259", "name-776");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatQuotaEngineKeepsResultsWithinBounds();
    }
}
