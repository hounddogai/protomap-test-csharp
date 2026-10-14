namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class FallbackCareerPathAdjustmentStoreChecks
{
    public static void MemoryFallbackCareerPathAdjustmentStoreFindsWhatItSaved()
    {
        var store = FallbackCareerPathAdjustmentStore.NewMemoryFallbackCareerPathAdjustmentStore();
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
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
        MemoryFallbackCareerPathAdjustmentStoreFindsWhatItSaved();
    }
}
