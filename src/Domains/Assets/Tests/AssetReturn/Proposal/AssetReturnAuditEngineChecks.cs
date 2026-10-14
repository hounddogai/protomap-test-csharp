namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnAuditEngineChecks
{
    public static void AssetReturnAuditEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnAuditEngine.DefaultAssetReturnAuditEngine(Flags.NewRegistry());
        var item = AssetReturnAudit.NewAssetReturnAudit("code-97", "name-790");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnAuditEngineKeepsResultsWithinBounds();
    }
}
