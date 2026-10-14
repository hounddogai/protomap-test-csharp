namespace Acme.Workforce.Compliance.Tests.Certification.Forecast;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class CertificationForecastStoreChecks
{
    public static void MemoryCertificationForecastStoreFindsWhatItSaved()
    {
        var store = CertificationForecastStore.NewMemoryCertificationForecastStore();
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
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
        MemoryCertificationForecastStoreFindsWhatItSaved();
    }
}
