namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationPlanStoreChecks
{
    public static void MemoryManualCertificationPlanStoreFindsWhatItSaved()
    {
        var store = ManualCertificationPlanStore.NewMemoryManualCertificationPlanStore();
        var item = ManualCertificationPlan.NewManualCertificationPlan("code-689", "name-20");
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
        MemoryManualCertificationPlanStoreFindsWhatItSaved();
    }
}
