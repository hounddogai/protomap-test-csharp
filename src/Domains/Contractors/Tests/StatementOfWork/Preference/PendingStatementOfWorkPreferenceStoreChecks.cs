namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingStatementOfWorkPreferenceStoreChecks
{
    public static void MemoryPendingStatementOfWorkPreferenceStoreFindsWhatItSaved()
    {
        var store = PendingStatementOfWorkPreferenceStore.NewMemoryPendingStatementOfWorkPreferenceStore();
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("code-457", "name-678");
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
        MemoryPendingStatementOfWorkPreferenceStoreFindsWhatItSaved();
    }
}
