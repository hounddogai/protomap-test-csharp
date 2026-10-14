namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanMetricStoreChecks
{
    public static void MemorySpanMetricStoreFindsWhatItSaved()
    {
        var store = SpanMetricStore.NewMemorySpanMetricStore();
        var item = SpanMetric.NewSpanMetric("code-110", "name-362");
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
        MemorySpanMetricStoreFindsWhatItSaved();
    }
}
