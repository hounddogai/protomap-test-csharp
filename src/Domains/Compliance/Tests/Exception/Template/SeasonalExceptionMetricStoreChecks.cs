namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalExceptionMetricStoreChecks
{
    public static void MemorySeasonalExceptionMetricStoreFindsWhatItSaved()
    {
        var store = SeasonalExceptionMetricStore.NewMemorySeasonalExceptionMetricStore();
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("code-386", "name-963");
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
        MemorySeasonalExceptionMetricStoreFindsWhatItSaved();
    }
}
