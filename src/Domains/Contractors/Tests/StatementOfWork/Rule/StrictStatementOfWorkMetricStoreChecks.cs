namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkMetricStoreChecks
{
    public static void MemoryStrictStatementOfWorkMetricStoreFindsWhatItSaved()
    {
        var store = StrictStatementOfWorkMetricStore.NewMemoryStrictStatementOfWorkMetricStore();
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("code-137", "name-494");
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
        MemoryStrictStatementOfWorkMetricStoreFindsWhatItSaved();
    }
}
