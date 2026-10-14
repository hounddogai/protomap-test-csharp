namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalBalanceDigestStoreChecks
{
    public static void MemoryRegionalBalanceDigestStoreFindsWhatItSaved()
    {
        var store = RegionalBalanceDigestStore.NewMemoryRegionalBalanceDigestStore();
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("code-294", "name-263");
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
        MemoryRegionalBalanceDigestStoreFindsWhatItSaved();
    }
}
