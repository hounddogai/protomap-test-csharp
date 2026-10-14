namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleStoreChecks
{
    public static void MemoryGlobalPolicyAckBundleStoreFindsWhatItSaved()
    {
        var store = GlobalPolicyAckBundleStore.NewMemoryGlobalPolicyAckBundleStore();
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
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
        MemoryGlobalPolicyAckBundleStoreFindsWhatItSaved();
    }
}
