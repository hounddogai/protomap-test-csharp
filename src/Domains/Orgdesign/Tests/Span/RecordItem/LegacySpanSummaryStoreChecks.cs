namespace Acme.Workforce.Orgdesign.Tests.Span.RecordItem;

using Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacySpanSummaryStoreChecks
{
    public static void MemoryLegacySpanSummaryStoreFindsWhatItSaved()
    {
        var store = LegacySpanSummaryStore.NewMemoryLegacySpanSummaryStore();
        var item = LegacySpanSummary.NewLegacySpanSummary("code-200", "name-128");
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
        MemoryLegacySpanSummaryStoreFindsWhatItSaved();
    }
}
