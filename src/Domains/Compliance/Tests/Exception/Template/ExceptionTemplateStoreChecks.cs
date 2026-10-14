namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionTemplateStoreChecks
{
    public static void MemoryExceptionTemplateStoreFindsWhatItSaved()
    {
        var store = ExceptionTemplateStore.NewMemoryExceptionTemplateStore();
        var item = ExceptionTemplate.NewExceptionTemplate("code-325", "title-116");
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
        MemoryExceptionTemplateStoreFindsWhatItSaved();
    }
}
