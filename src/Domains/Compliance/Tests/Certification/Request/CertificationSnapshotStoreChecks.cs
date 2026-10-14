namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationSnapshotStoreChecks
{
    public static void MemoryCertificationSnapshotStoreFindsWhatItSaved()
    {
        var store = CertificationSnapshotStore.NewMemoryCertificationSnapshotStore();
        var item = CertificationSnapshot.NewCertificationSnapshot("code-42", "label-390");
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
        MemoryCertificationSnapshotStoreFindsWhatItSaved();
    }
}
