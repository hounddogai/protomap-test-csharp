namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolMetricStoreChecks
{
    public static void MemoryBonusPoolMetricStoreFindsWhatItSaved()
    {
        var store = BonusPoolMetricStore.NewMemoryBonusPoolMetricStore();
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
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
        MemoryBonusPoolMetricStoreFindsWhatItSaved();
    }
}
