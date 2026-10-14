namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AssetReturnTemplateChecks
{
    public static void AssetReturnTemplateStartsInNew()
    {
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        Check.Equal(item.Status, AssetReturnTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnTemplateMovesToDone()
    {
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        try
        {
            item.MoveTo(AssetReturnTemplateStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnTemplateStatus.Done, "status");
    }

    public static void AssetReturnTemplateRejectsMovesFromFailed()
    {
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        item.Status = AssetReturnTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnTemplateStartsInNew();
        AssetReturnTemplateMovesToDone();
        AssetReturnTemplateRejectsMovesFromFailed();
    }
}
