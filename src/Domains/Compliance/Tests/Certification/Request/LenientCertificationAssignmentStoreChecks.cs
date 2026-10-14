namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientCertificationAssignmentStoreChecks
{
    public static void MemoryLenientCertificationAssignmentStoreFindsWhatItSaved()
    {
        var store = LenientCertificationAssignmentStore.NewMemoryLenientCertificationAssignmentStore();
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("code-515", "title-388");
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
        MemoryLenientCertificationAssignmentStoreFindsWhatItSaved();
    }
}
