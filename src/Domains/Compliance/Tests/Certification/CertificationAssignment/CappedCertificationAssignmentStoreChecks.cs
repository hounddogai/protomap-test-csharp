namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentStoreChecks
{
    public static void MemoryCappedCertificationAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedCertificationAssignmentStore.NewMemoryCappedCertificationAssignmentStore();
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
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
        MemoryCappedCertificationAssignmentStoreFindsWhatItSaved();
    }
}
