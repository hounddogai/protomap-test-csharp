namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class AccrualDraftStoreChecks
{
    public static void MemoryAccrualDraftStoreFindsWhatItSaved()
    {
        var store = AccrualDraftStore.NewMemoryAccrualDraftStore();
        var item = AccrualDraft.NewAccrualDraft("code-437", "label-267");
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
        MemoryAccrualDraftStoreFindsWhatItSaved();
    }
}
