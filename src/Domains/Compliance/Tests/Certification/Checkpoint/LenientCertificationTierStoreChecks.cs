namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientCertificationTierStoreChecks
{
    public static void MemoryLenientCertificationTierStoreFindsWhatItSaved()
    {
        var store = LenientCertificationTierStore.NewMemoryLenientCertificationTierStore();
        var item = LenientCertificationTier.NewLenientCertificationTier("code-459", "name-677");
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
        MemoryLenientCertificationTierStoreFindsWhatItSaved();
    }
}
