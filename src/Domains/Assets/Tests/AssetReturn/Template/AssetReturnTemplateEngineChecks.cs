namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AssetReturnTemplateEngineChecks
{
    public static void AssetReturnTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnTemplateEngine.DefaultAssetReturnTemplateEngine(Flags.NewRegistry());
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnTemplateEngineKeepsResultsWithinBounds();
    }
}
