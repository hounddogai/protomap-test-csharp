namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AssetReturnTemplateStoreChecks
{
    public static void MemoryAssetReturnTemplateStoreFindsWhatItSaved()
    {
        var store = AssetReturnTemplateStore.NewMemoryAssetReturnTemplateStore();
        var item = AssetReturnTemplate.NewAssetReturnTemplate("code-237", "label-543");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryAssetReturnTemplateStoreFindsWhatItSaved();
    }
}
