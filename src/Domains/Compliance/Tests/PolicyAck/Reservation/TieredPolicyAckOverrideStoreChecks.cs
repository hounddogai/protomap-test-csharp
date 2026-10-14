namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideStoreChecks
{
    public static void MemoryTieredPolicyAckOverrideStoreFindsWhatItSaved()
    {
        var store = TieredPolicyAckOverrideStore.NewMemoryTieredPolicyAckOverrideStore();
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("code-69", "label-144");
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
        MemoryTieredPolicyAckOverrideStoreFindsWhatItSaved();
    }
}
