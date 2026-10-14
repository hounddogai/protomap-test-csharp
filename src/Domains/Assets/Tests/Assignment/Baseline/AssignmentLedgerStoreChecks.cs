namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentLedgerStoreChecks
{
    public static void MemoryAssignmentLedgerStoreFindsWhatItSaved()
    {
        var store = AssignmentLedgerStore.NewMemoryAssignmentLedgerStore();
        var item = AssignmentLedger.NewAssignmentLedger("code-998", "label-957");
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
        MemoryAssignmentLedgerStoreFindsWhatItSaved();
    }
}
