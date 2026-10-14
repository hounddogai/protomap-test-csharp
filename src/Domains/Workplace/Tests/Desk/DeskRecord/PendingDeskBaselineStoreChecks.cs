namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class PendingDeskBaselineStoreChecks
{
    public static void MemoryPendingDeskBaselineStoreFindsWhatItSaved()
    {
        var store = PendingDeskBaselineStore.NewMemoryPendingDeskBaselineStore();
        var item = PendingDeskBaseline.NewPendingDeskBaseline("code-666", "name-638");
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
        MemoryPendingDeskBaselineStoreFindsWhatItSaved();
    }
}
