namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalPositionVarianceStoreChecks
{
    public static void MemoryGlobalPositionVarianceStoreFindsWhatItSaved()
    {
        var store = GlobalPositionVarianceStore.NewMemoryGlobalPositionVarianceStore();
        var item = GlobalPositionVariance.NewGlobalPositionVariance("code-155", "title-406");
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
        MemoryGlobalPositionVarianceStoreFindsWhatItSaved();
    }
}
