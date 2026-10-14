namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientGarnishmentSlotEngineChecks
{
    public static void LenientGarnishmentSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LenientGarnishmentSlotEngine.DefaultLenientGarnishmentSlotEngine();
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("code-657", "title-494");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientGarnishmentSlotEngineKeepsResultsWithinBounds();
    }
}
