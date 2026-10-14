namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordStoreChecks
{
    public static void MemoryAdaptiveHeadcountRecordStoreFindsWhatItSaved()
    {
        var store = AdaptiveHeadcountRecordStore.NewMemoryAdaptiveHeadcountRecordStore();
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
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
        MemoryAdaptiveHeadcountRecordStoreFindsWhatItSaved();
    }
}
