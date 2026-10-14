namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordStoreChecks
{
    public static void MemoryPolicyRecordStoreFindsWhatItSaved()
    {
        var store = PolicyRecordStore.NewMemoryPolicyRecordStore();
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
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
        MemoryPolicyRecordStoreFindsWhatItSaved();
    }
}
