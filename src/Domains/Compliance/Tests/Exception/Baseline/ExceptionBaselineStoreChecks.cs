namespace Acme.Workforce.Compliance.Tests.Exception.Baseline;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ExceptionBaselineStoreChecks
{
    public static void MemoryExceptionBaselineStoreFindsWhatItSaved()
    {
        var store = ExceptionBaselineStore.NewMemoryExceptionBaselineStore();
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
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
        MemoryExceptionBaselineStoreFindsWhatItSaved();
    }
}
