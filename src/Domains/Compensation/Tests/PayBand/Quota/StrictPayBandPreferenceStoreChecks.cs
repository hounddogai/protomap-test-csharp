namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictPayBandPreferenceStoreChecks
{
    public static void MemoryStrictPayBandPreferenceStoreFindsWhatItSaved()
    {
        var store = StrictPayBandPreferenceStore.NewMemoryStrictPayBandPreferenceStore();
        var item = StrictPayBandPreference.NewStrictPayBandPreference("code-527", "name-204");
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
        MemoryStrictPayBandPreferenceStoreFindsWhatItSaved();
    }
}
