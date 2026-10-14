namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Metric;

using Acme.Workforce.Contractors.StatementOfWork.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMetricStoreChecks
{
    public static void MemoryDefaultStatementOfWorkMetricStoreFindsWhatItSaved()
    {
        var store = DefaultStatementOfWorkMetricStore.NewMemoryDefaultStatementOfWorkMetricStore();
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("code-732", "name-210");
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
        MemoryDefaultStatementOfWorkMetricStoreFindsWhatItSaved();
    }
}
