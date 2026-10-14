namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckManifestStoreChecks
{
    public static void MemoryPolicyAckManifestStoreFindsWhatItSaved()
    {
        var store = PolicyAckManifestStore.NewMemoryPolicyAckManifestStore();
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
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
        MemoryPolicyAckManifestStoreFindsWhatItSaved();
    }
}
