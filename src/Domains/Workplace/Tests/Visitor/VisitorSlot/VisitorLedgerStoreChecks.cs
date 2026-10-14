namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorLedgerStoreChecks
{
    public static void MemoryVisitorLedgerStoreFindsWhatItSaved()
    {
        var store = VisitorLedgerStore.NewMemoryVisitorLedgerStore();
        var item = VisitorLedger.NewVisitorLedger("code-295", "title-921");
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
        MemoryVisitorLedgerStoreFindsWhatItSaved();
    }
}
