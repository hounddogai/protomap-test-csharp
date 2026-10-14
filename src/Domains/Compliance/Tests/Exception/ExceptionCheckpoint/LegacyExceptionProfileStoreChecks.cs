namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class LegacyExceptionProfileStoreChecks
{
    public static void MemoryLegacyExceptionProfileStoreFindsWhatItSaved()
    {
        var store = LegacyExceptionProfileStore.NewMemoryLegacyExceptionProfileStore();
        var item = LegacyExceptionProfile.NewLegacyExceptionProfile("code-294", "label-383");
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
        MemoryLegacyExceptionProfileStoreFindsWhatItSaved();
    }
}
