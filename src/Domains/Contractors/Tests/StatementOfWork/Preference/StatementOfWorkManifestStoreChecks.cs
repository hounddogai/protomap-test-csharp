namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkManifestStoreChecks
{
    public static void MemoryStatementOfWorkManifestStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkManifestStore.NewMemoryStatementOfWorkManifestStore();
        var item = StatementOfWorkManifest.NewStatementOfWorkManifest("code-714", "title-907");
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
        MemoryStatementOfWorkManifestStoreFindsWhatItSaved();
    }
}
