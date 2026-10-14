namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalAccrualDigestStoreChecks
{
    public static void MemorySeasonalAccrualDigestStoreFindsWhatItSaved()
    {
        var store = SeasonalAccrualDigestStore.NewMemorySeasonalAccrualDigestStore();
        var item = SeasonalAccrualDigest.NewSeasonalAccrualDigest("code-94", "name-598");
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
        MemorySeasonalAccrualDigestStoreFindsWhatItSaved();
    }
}
