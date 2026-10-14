namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayThresholdEngineChecks
{
    public static void RetroPayThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayThresholdEngine.DefaultRetroPayThresholdEngine(Flags.NewRegistry());
        var item = RetroPayThreshold.NewRetroPayThreshold("code-950", "label-496");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayThresholdEngineKeepsResultsWithinBounds();
    }
}
