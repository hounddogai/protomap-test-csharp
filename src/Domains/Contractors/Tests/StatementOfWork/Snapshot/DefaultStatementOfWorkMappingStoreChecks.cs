namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMappingStoreChecks
{
    public static void MemoryDefaultStatementOfWorkMappingStoreFindsWhatItSaved()
    {
        var store = DefaultStatementOfWorkMappingStore.NewMemoryDefaultStatementOfWorkMappingStore();
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
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
        MemoryDefaultStatementOfWorkMappingStoreFindsWhatItSaved();
    }
}
