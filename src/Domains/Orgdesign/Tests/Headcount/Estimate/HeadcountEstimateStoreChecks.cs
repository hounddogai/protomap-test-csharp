namespace Acme.Workforce.Orgdesign.Tests.Headcount.Estimate;

using Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class HeadcountEstimateStoreChecks
{
    public static void MemoryHeadcountEstimateStoreFindsWhatItSaved()
    {
        var store = HeadcountEstimateStore.NewMemoryHeadcountEstimateStore();
        var item = HeadcountEstimate.NewHeadcountEstimate("code-437", "label-491");
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
        MemoryHeadcountEstimateStoreFindsWhatItSaved();
    }
}
