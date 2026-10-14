namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class DefaultWarrantyCycleEngineChecks
{
    public static void DefaultWarrantyCycleEngineExtraKeepsResultsWithinBounds()
    {
        var engine = DefaultWarrantyCycleEngine.DefaultDefaultWarrantyCycleEngine(Flags.NewRegistry());
        var item = DefaultWarrantyCycle.NewDefaultWarrantyCycle("code-226", "name-125");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultWarrantyCycleEngineExtraKeepsResultsWithinBounds();
    }
}
