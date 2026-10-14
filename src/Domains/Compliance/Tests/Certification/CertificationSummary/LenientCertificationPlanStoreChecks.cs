namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class LenientCertificationPlanStoreChecks
{
    public static void MemoryLenientCertificationPlanStoreFindsWhatItSaved()
    {
        var store = LenientCertificationPlanStore.NewMemoryLenientCertificationPlanStore();
        var item = LenientCertificationPlan.NewLenientCertificationPlan("code-128", "label-8");
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
        MemoryLenientCertificationPlanStoreFindsWhatItSaved();
    }
}
