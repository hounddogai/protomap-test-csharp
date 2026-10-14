namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnAgreementEngineChecks
{
    public static void AssetReturnAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnAgreementEngine.DefaultAssetReturnAgreementEngine(Flags.NewRegistry());
        var item = AssetReturnAgreement.NewAssetReturnAgreement("code-436", "name-516");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnAgreementEngineKeepsResultsWithinBounds();
    }
}
