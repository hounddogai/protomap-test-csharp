namespace Acme.Workforce.Compliance.Tests.Attestation.Baseline;

using Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalAttestationBaselineStoreChecks
{
    public static void MemoryGlobalAttestationBaselineStoreFindsWhatItSaved()
    {
        var store = GlobalAttestationBaselineStore.NewMemoryGlobalAttestationBaselineStore();
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("code-731", "label-485");
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
        MemoryGlobalAttestationBaselineStoreFindsWhatItSaved();
    }
}
