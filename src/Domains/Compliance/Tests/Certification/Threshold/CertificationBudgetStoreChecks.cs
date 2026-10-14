namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationBudgetStoreChecks
{
    public static void MemoryCertificationBudgetStoreFindsWhatItSaved()
    {
        var store = CertificationBudgetStore.NewMemoryCertificationBudgetStore();
        var item = CertificationBudget.NewCertificationBudget("code-837", "label-810");
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
        MemoryCertificationBudgetStoreFindsWhatItSaved();
    }
}
