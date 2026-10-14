namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationRequestStoreChecks
{
    public static void MemoryCertificationRequestStoreFindsWhatItSaved()
    {
        var store = CertificationRequestStore.NewMemoryCertificationRequestStore();
        var item = CertificationRequest.NewCertificationRequest("code-463", "title-550");
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
        MemoryCertificationRequestStoreFindsWhatItSaved();
    }
}
