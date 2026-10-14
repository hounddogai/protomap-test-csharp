namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnAgreementStoreChecks
{
    public static void MemoryAssetReturnAgreementStoreFindsWhatItSaved()
    {
        var store = AssetReturnAgreementStore.NewMemoryAssetReturnAgreementStore();
        var item = AssetReturnAgreement.NewAssetReturnAgreement("code-436", "name-516");
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
        MemoryAssetReturnAgreementStoreFindsWhatItSaved();
    }
}
