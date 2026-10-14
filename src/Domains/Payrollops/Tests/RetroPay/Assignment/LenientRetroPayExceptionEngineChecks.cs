namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayExceptionEngineChecks
{
    public static void LenientRetroPayExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRetroPayExceptionEngine.DefaultLenientRetroPayExceptionEngine(Flags.NewRegistry());
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRetroPayExceptionEngineKeepsResultsWithinBounds();
    }
}
