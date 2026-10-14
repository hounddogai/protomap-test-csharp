namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class DefaultPolicyCycleStoreChecks
{
    public static void MemoryDefaultPolicyCycleStoreFindsWhatItSaved()
    {
        var store = DefaultPolicyCycleStore.NewMemoryDefaultPolicyCycleStore();
        var item = DefaultPolicyCycle.NewDefaultPolicyCycle("code-618", "title-651");
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
        MemoryDefaultPolicyCycleStoreFindsWhatItSaved();
    }
}
