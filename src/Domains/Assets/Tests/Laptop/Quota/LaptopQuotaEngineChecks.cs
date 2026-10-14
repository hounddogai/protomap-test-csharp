namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopQuotaEngineChecks
{
    public static void LaptopQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopQuotaEngine.DefaultLaptopQuotaEngine(Flags.NewRegistry());
        var item = LaptopQuota.NewLaptopQuota("code-370", "label-726");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopQuotaEngineKeepsResultsWithinBounds();
    }
}
