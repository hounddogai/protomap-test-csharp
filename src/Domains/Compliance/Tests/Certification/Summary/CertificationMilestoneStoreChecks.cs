namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationMilestoneStoreChecks
{
    public static void MemoryCertificationMilestoneStoreFindsWhatItSaved()
    {
        var store = CertificationMilestoneStore.NewMemoryCertificationMilestoneStore();
        var item = CertificationMilestone.NewCertificationMilestone("code-557", "title-371");
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
        MemoryCertificationMilestoneStoreFindsWhatItSaved();
    }
}
