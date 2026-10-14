namespace Acme.Workforce.Compensation.Tests.EquityGrant.Plan;

using Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedEquityGrantPreferenceStoreChecks
{
    public static void MemoryBatchedEquityGrantPreferenceStoreFindsWhatItSaved()
    {
        var store = BatchedEquityGrantPreferenceStore.NewMemoryBatchedEquityGrantPreferenceStore();
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
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
        MemoryBatchedEquityGrantPreferenceStoreFindsWhatItSaved();
    }
}
