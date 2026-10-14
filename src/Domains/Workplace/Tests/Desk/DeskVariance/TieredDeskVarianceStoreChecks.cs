namespace Acme.Workforce.Workplace.Tests.Desk.DeskVariance;

using Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>Checks of desk variance code, run by the project's runner.</summary>
public static class TieredDeskVarianceStoreChecks
{
    public static void MemoryTieredDeskVarianceStoreFindsWhatItSaved()
    {
        var store = TieredDeskVarianceStore.NewMemoryTieredDeskVarianceStore();
        var item = TieredDeskVariance.NewTieredDeskVariance("code-80", "label-65");
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
        MemoryTieredDeskVarianceStoreFindsWhatItSaved();
    }
}
