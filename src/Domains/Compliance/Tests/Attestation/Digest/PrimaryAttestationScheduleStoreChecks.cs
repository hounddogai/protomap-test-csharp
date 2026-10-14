namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryAttestationScheduleStoreChecks
{
    public static void MemoryPrimaryAttestationScheduleStoreFindsWhatItSaved()
    {
        var store = PrimaryAttestationScheduleStore.NewMemoryPrimaryAttestationScheduleStore();
        var item = PrimaryAttestationSchedule.NewPrimaryAttestationSchedule("code-304", "name-38");
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
        MemoryPrimaryAttestationScheduleStoreFindsWhatItSaved();
    }
}
