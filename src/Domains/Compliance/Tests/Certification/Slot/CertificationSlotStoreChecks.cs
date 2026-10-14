namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationSlotStoreChecks
{
    public static void MemoryCertificationSlotStoreFindsWhatItSaved()
    {
        var store = CertificationSlotStore.NewMemoryCertificationSlotStore();
        var item = CertificationSlot.NewCertificationSlot("code-958", "title-326");
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
        MemoryCertificationSlotStoreFindsWhatItSaved();
    }
}
