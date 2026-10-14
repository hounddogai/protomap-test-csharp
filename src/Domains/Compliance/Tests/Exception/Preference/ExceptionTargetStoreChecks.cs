namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ExceptionTargetStoreChecks
{
    public static void MemoryExceptionTargetStoreFindsWhatItSaved()
    {
        var store = ExceptionTargetStore.NewMemoryExceptionTargetStore();
        var item = ExceptionTarget.NewExceptionTarget("code-359", "label-952");
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
        MemoryExceptionTargetStoreFindsWhatItSaved();
    }
}
