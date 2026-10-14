namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class PositionVarianceStoreChecks
{
    public static void MemoryPositionVarianceStoreFindsWhatItSaved()
    {
        var store = PositionVarianceStore.NewMemoryPositionVarianceStore();
        var item = PositionVariance.NewPositionVariance("code-991", "label-899");
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
        MemoryPositionVarianceStoreFindsWhatItSaved();
    }
}
