namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class LenientPayBandScoreStoreChecks
{
    public static void MemoryLenientPayBandScoreStoreFindsWhatItSaved()
    {
        var store = LenientPayBandScoreStore.NewMemoryLenientPayBandScoreStore();
        var item = LenientPayBandScore.NewLenientPayBandScore("code-467", "label-779");
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
        MemoryLenientPayBandScoreStoreFindsWhatItSaved();
    }
}
