namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientRetroPayRollupEngineChecks
{
    public static void LenientRetroPayRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRetroPayRollupEngine.DefaultLenientRetroPayRollupEngine(Flags.NewRegistry());
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("code-201", "title-469");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRetroPayRollupEngineKeepsResultsWithinBounds();
    }
}
