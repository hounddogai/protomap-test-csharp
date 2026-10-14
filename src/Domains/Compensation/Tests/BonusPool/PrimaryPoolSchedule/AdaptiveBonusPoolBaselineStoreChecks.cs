namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineStoreChecks
{
    public static void MemoryAdaptiveBonusPoolBaselineStoreFindsWhatItSaved()
    {
        var store = AdaptiveBonusPoolBaselineStore.NewMemoryAdaptiveBonusPoolBaselineStore();
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
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
        MemoryAdaptiveBonusPoolBaselineStoreFindsWhatItSaved();
    }
}
