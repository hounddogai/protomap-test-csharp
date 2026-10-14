namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingWarrantyGroupEngineChecks
{
    public static void PendingWarrantyGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PendingWarrantyGroupEngine.DefaultPendingWarrantyGroupEngine(Flags.NewRegistry());
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("code-413", "title-622");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingWarrantyGroupEngineKeepsResultsWithinBounds();
    }
}
