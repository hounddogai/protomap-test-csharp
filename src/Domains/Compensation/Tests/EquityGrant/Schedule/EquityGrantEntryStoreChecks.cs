namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantEntryStoreChecks
{
    public static void MemoryEquityGrantEntryStoreFindsWhatItSaved()
    {
        var store = EquityGrantEntryStore.NewMemoryEquityGrantEntryStore();
        var item = EquityGrantEntry.NewEquityGrantEntry("code-165", "label-722");
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
        MemoryEquityGrantEntryStoreFindsWhatItSaved();
    }
}
