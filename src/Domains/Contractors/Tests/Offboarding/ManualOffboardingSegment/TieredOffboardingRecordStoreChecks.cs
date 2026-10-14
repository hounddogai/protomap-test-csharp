namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class TieredOffboardingRecordStoreChecks
{
    public static void MemoryTieredOffboardingRecordStoreFindsWhatItSaved()
    {
        var store = TieredOffboardingRecordStore.NewMemoryTieredOffboardingRecordStore();
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("code-33", "name-610");
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
        MemoryTieredOffboardingRecordStoreFindsWhatItSaved();
    }
}
