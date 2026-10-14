namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PrimaryPoolOverrideStoreChecks
{
    public static void MemoryPrimaryPoolOverrideStoreFindsWhatItSaved()
    {
        var store = PrimaryPoolOverrideStore.NewMemoryPrimaryPoolOverrideStore();
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
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
        MemoryPrimaryPoolOverrideStoreFindsWhatItSaved();
    }
}
