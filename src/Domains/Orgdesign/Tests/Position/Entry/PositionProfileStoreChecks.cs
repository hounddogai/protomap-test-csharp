namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionProfileStoreChecks
{
    public static void MemoryPositionProfileStoreFindsWhatItSaved()
    {
        var store = PositionProfileStore.NewMemoryPositionProfileStore();
        var item = PositionProfile.NewPositionProfile("code-607", "name-436");
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
        MemoryPositionProfileStoreFindsWhatItSaved();
    }
}
