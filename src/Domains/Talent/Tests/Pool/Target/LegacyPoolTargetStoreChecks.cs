namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPoolTargetStoreChecks
{
    public static void MemoryLegacyPoolTargetStoreFindsWhatItSaved()
    {
        var store = LegacyPoolTargetStore.NewMemoryLegacyPoolTargetStore();
        var item = LegacyPoolTarget.NewLegacyPoolTarget("code-747", "name-634");
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
        MemoryLegacyPoolTargetStoreFindsWhatItSaved();
    }
}
