namespace Acme.Workforce.Compliance.Tests.Certification.Template;

using Acme.Workforce.Compliance.Certification.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CertificationPlanStoreChecks
{
    public static void MemoryCertificationPlanStoreFindsWhatItSaved()
    {
        var store = CertificationPlanStore.NewMemoryCertificationPlanStore();
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
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
        MemoryCertificationPlanStoreFindsWhatItSaved();
    }
}
