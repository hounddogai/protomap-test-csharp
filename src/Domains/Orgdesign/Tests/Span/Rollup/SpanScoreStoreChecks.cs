namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanScoreStoreChecks
{
    public static void MemorySpanScoreStoreFindsWhatItSaved()
    {
        var store = SpanScoreStore.NewMemorySpanScoreStore();
        var item = SpanScore.NewSpanScore("code-175", "title-609");
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
        MemorySpanScoreStoreFindsWhatItSaved();
    }
}
