namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileStoreChecks
{
    public static void MemoryPrimaryPositionProfileStoreFindsWhatItSaved()
    {
        var store = PrimaryPositionProfileStore.NewMemoryPrimaryPositionProfileStore();
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
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
        MemoryPrimaryPositionProfileStoreFindsWhatItSaved();
    }
}
