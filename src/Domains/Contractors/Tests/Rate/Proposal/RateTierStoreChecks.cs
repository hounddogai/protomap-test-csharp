namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateTierStoreChecks
{
    public static void MemoryRateTierStoreFindsWhatItSaved()
    {
        var store = RateTierStore.NewMemoryRateTierStore();
        var item = RateTier.NewRateTier("code-101", "name-832");
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
        MemoryRateTierStoreFindsWhatItSaved();
    }
}
