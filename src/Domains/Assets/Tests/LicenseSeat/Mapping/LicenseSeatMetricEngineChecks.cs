namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricEngineChecks
{
    public static void LicenseSeatMetricEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatMetricEngine.DefaultLicenseSeatMetricEngine(Flags.NewRegistry());
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatMetricEngineKeepsResultsWithinBounds();
    }
}
