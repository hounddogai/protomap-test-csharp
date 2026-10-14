namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetStoreChecks
{
    public static void MemorySpanBudgetStoreFindsWhatItSaved()
    {
        var store = SpanBudgetStore.NewMemorySpanBudgetStore();
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
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
        MemorySpanBudgetStoreFindsWhatItSaved();
    }
}
