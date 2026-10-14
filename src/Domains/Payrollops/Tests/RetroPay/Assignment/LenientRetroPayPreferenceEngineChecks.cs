namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceEngineChecks
{
    public static void LenientRetroPayPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRetroPayPreferenceEngine.DefaultLenientRetroPayPreferenceEngine(Flags.NewRegistry());
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRetroPayPreferenceEngineKeepsResultsWithinBounds();
    }
}
