namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentSlotEngineChecks
{
    public static void LegacyGarnishmentSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyGarnishmentSlotEngine.DefaultLegacyGarnishmentSlotEngine();
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("code-718", "label-569");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyGarnishmentSlotEngineKeepsResultsWithinBounds();
    }
}
