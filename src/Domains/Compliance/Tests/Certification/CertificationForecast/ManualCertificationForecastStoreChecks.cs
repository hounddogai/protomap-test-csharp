namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationForecastStoreChecks
{
    public static void MemoryManualCertificationForecastStoreFindsWhatItSaved()
    {
        var store = ManualCertificationForecastStore.NewMemoryManualCertificationForecastStore();
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
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
        MemoryManualCertificationForecastStoreFindsWhatItSaved();
    }
}
