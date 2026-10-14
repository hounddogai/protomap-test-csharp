namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanTemplateStoreChecks
{
    public static void MemorySpanTemplateStoreFindsWhatItSaved()
    {
        var store = SpanTemplateStore.NewMemorySpanTemplateStore();
        var item = SpanTemplate.NewSpanTemplate("code-852", "label-134");
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
        MemorySpanTemplateStoreFindsWhatItSaved();
    }
}
