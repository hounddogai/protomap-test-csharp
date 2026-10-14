namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalDeskVarianceStoreChecks
{
    public static void MemoryGlobalDeskVarianceStoreFindsWhatItSaved()
    {
        var store = GlobalDeskVarianceStore.NewMemoryGlobalDeskVarianceStore();
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
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
        MemoryGlobalDeskVarianceStoreFindsWhatItSaved();
    }
}
