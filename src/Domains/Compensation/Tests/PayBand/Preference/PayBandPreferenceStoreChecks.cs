namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandPreferenceStoreChecks
{
    public static void MemoryPayBandPreferenceStoreFindsWhatItSaved()
    {
        var store = PayBandPreferenceStore.NewMemoryPayBandPreferenceStore();
        var item = PayBandPreference.NewPayBandPreference("code-301", "title-777");
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
        MemoryPayBandPreferenceStoreFindsWhatItSaved();
    }
}
