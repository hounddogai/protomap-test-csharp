namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledReorgVarianceStoreChecks
{
    public static void MemoryScheduledReorgVarianceStoreFindsWhatItSaved()
    {
        var store = ScheduledReorgVarianceStore.NewMemoryScheduledReorgVarianceStore();
        var item = ScheduledReorgVariance.NewScheduledReorgVariance("code-363", "label-857");
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
        MemoryScheduledReorgVarianceStoreFindsWhatItSaved();
    }
}
