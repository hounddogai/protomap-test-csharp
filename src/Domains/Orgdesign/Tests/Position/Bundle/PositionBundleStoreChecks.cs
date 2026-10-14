namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleStoreChecks
{
    public static void MemoryPositionBundleStoreFindsWhatItSaved()
    {
        var store = PositionBundleStore.NewMemoryPositionBundleStore();
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
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
        MemoryPositionBundleStoreFindsWhatItSaved();
    }
}
