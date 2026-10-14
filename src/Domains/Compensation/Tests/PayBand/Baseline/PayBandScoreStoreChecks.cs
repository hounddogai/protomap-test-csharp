namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreStoreChecks
{
    public static void MemoryPayBandScoreStoreFindsWhatItSaved()
    {
        var store = PayBandScoreStore.NewMemoryPayBandScoreStore();
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
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
        MemoryPayBandScoreStoreFindsWhatItSaved();
    }
}
