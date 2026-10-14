namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBonusPoolDraftStoreChecks
{
    public static void MemoryManualBonusPoolDraftStoreFindsWhatItSaved()
    {
        var store = ManualBonusPoolDraftStore.NewMemoryManualBonusPoolDraftStore();
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("code-986", "name-756");
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
        MemoryManualBonusPoolDraftStoreFindsWhatItSaved();
    }
}
