namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class RateBaselineStoreChecks
{
    public static void MemoryRateBaselineStoreFindsWhatItSaved()
    {
        var store = RateBaselineStore.NewMemoryRateBaselineStore();
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
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
        MemoryRateBaselineStoreFindsWhatItSaved();
    }
}
