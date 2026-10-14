namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ExceptionProfileStoreChecks
{
    public static void MemoryExceptionProfileStoreFindsWhatItSaved()
    {
        var store = ExceptionProfileStore.NewMemoryExceptionProfileStore();
        var item = ExceptionProfile.NewExceptionProfile("code-888", "name-435");
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
        MemoryExceptionProfileStoreFindsWhatItSaved();
    }
}
