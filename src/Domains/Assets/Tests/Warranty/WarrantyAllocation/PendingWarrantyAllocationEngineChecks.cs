namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class PendingWarrantyAllocationEngineChecks
{
    public static void PendingWarrantyAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PendingWarrantyAllocationEngine.DefaultPendingWarrantyAllocationEngine();
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("code-596", "title-281");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingWarrantyAllocationEngineKeepsResultsWithinBounds();
    }
}
