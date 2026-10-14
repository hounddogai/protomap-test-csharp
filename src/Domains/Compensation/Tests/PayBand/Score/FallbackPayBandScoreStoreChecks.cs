namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FallbackPayBandScoreStoreChecks
{
    public static void MemoryFallbackPayBandScoreStoreFindsWhatItSaved()
    {
        var store = FallbackPayBandScoreStore.NewMemoryFallbackPayBandScoreStore();
        var item = FallbackPayBandScore.NewFallbackPayBandScore("code-180", "name-235");
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
        MemoryFallbackPayBandScoreStoreFindsWhatItSaved();
    }
}
