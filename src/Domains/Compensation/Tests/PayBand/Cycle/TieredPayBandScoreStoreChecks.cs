namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TieredPayBandScoreStoreChecks
{
    public static void MemoryTieredPayBandScoreStoreFindsWhatItSaved()
    {
        var store = TieredPayBandScoreStore.NewMemoryTieredPayBandScoreStore();
        var item = TieredPayBandScore.NewTieredPayBandScore("code-986", "title-910");
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
        MemoryTieredPayBandScoreStoreFindsWhatItSaved();
    }
}
