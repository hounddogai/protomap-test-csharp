namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CostModelLedgerStoreChecks
{
    public static void MemoryCostModelLedgerStoreFindsWhatItSaved()
    {
        var store = CostModelLedgerStore.NewMemoryCostModelLedgerStore();
        var item = CostModelLedger.NewCostModelLedger("code-841", "label-66");
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
        MemoryCostModelLedgerStoreFindsWhatItSaved();
    }
}
