namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanWindowStoreChecks
{
    public static void MemorySpanWindowStoreFindsWhatItSaved()
    {
        var store = SpanWindowStore.NewMemorySpanWindowStore();
        var item = SpanWindow.NewSpanWindow("code-258", "title-6");
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
        MemorySpanWindowStoreFindsWhatItSaved();
    }
}
