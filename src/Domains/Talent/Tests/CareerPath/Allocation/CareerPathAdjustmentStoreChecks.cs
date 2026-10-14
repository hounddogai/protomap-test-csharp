namespace Acme.Workforce.Talent.Tests.CareerPath.Allocation;

using Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class CareerPathAdjustmentStoreChecks
{
    public static void MemoryCareerPathAdjustmentStoreFindsWhatItSaved()
    {
        var store = CareerPathAdjustmentStore.NewMemoryCareerPathAdjustmentStore();
        var item = CareerPathAdjustment.NewCareerPathAdjustment("code-447", "title-343");
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
        MemoryCareerPathAdjustmentStoreFindsWhatItSaved();
    }
}
