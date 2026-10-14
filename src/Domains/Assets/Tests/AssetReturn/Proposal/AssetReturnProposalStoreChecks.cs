namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnProposalStoreChecks
{
    public static void MemoryAssetReturnProposalStoreFindsWhatItSaved()
    {
        var store = AssetReturnProposalStore.NewMemoryAssetReturnProposalStore();
        var item = AssetReturnProposal.NewAssetReturnProposal("code-137", "label-277");
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
        MemoryAssetReturnProposalStoreFindsWhatItSaved();
    }
}
