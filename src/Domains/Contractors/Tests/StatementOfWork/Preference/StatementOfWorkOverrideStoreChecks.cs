namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideStoreChecks
{
    public static void MemoryStatementOfWorkOverrideStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkOverrideStore.NewMemoryStatementOfWorkOverrideStore();
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
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
        MemoryStatementOfWorkOverrideStoreFindsWhatItSaved();
    }
}
