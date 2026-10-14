namespace Acme.Workforce.Orgdesign.Tests.Headcount.Rollup;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class HeadcountMetricStoreChecks
{
    public static void MemoryHeadcountMetricStoreFindsWhatItSaved()
    {
        var store = HeadcountMetricStore.NewMemoryHeadcountMetricStore();
        var item = HeadcountMetric.NewHeadcountMetric("code-111", "name-767");
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
        MemoryHeadcountMetricStoreFindsWhatItSaved();
    }
}
