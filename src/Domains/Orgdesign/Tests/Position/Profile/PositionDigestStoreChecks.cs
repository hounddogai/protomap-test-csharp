namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionDigestStoreChecks
{
    public static void MemoryPositionDigestStoreFindsWhatItSaved()
    {
        var store = PositionDigestStore.NewMemoryPositionDigestStore();
        var item = PositionDigest.NewPositionDigest("code-587", "name-354");
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
        MemoryPositionDigestStoreFindsWhatItSaved();
    }
}
