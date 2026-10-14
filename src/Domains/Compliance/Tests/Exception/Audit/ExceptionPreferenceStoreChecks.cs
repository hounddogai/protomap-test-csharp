namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ExceptionPreferenceStoreChecks
{
    public static void MemoryExceptionPreferenceStoreFindsWhatItSaved()
    {
        var store = ExceptionPreferenceStore.NewMemoryExceptionPreferenceStore();
        var item = ExceptionPreference.NewExceptionPreference("code-264", "title-371");
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
        MemoryExceptionPreferenceStoreFindsWhatItSaved();
    }
}
