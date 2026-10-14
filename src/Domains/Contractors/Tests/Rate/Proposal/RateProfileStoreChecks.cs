namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateProfileStoreChecks
{
    public static void MemoryRateProfileStoreFindsWhatItSaved()
    {
        var store = RateProfileStore.NewMemoryRateProfileStore();
        var item = RateProfile.NewRateProfile("code-138", "title-450");
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
        MemoryRateProfileStoreFindsWhatItSaved();
    }
}
