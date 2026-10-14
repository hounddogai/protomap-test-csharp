namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyHolidayBundleStoreChecks
{
    public static void MemoryLegacyHolidayBundleStoreFindsWhatItSaved()
    {
        var store = LegacyHolidayBundleStore.NewMemoryLegacyHolidayBundleStore();
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("code-592", "title-338");
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
        MemoryLegacyHolidayBundleStoreFindsWhatItSaved();
    }
}
