namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AttestationPlanStoreChecks
{
    public static void MemoryAttestationPlanStoreFindsWhatItSaved()
    {
        var store = AttestationPlanStore.NewMemoryAttestationPlanStore();
        var item = AttestationPlan.NewAttestationPlan("code-759", "title-681");
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
        MemoryAttestationPlanStoreFindsWhatItSaved();
    }
}
