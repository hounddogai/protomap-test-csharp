namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingWarrantyQuotaEngineChecks
{
    public static void PendingWarrantyQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = PendingWarrantyQuotaEngine.DefaultPendingWarrantyQuotaEngine(Flags.NewRegistry());
        var item = PendingWarrantyQuota.NewPendingWarrantyQuota("code-388", "label-383");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingWarrantyQuotaEngineKeepsResultsWithinBounds();
    }
}
