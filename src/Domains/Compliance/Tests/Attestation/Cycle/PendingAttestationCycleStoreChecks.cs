namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingAttestationCycleStoreChecks
{
    public static void MemoryPendingAttestationCycleStoreFindsWhatItSaved()
    {
        var store = PendingAttestationCycleStore.NewMemoryPendingAttestationCycleStore();
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
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
        MemoryPendingAttestationCycleStoreFindsWhatItSaved();
    }
}
