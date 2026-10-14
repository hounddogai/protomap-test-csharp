namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PolicyAckLedgerStoreChecks
{
    public static void MemoryPolicyAckLedgerStoreFindsWhatItSaved()
    {
        var store = PolicyAckLedgerStore.NewMemoryPolicyAckLedgerStore();
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
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
        MemoryPolicyAckLedgerStoreFindsWhatItSaved();
    }
}
