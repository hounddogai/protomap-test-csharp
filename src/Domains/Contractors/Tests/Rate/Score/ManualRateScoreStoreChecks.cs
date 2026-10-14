namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ManualRateScoreStoreChecks
{
    public static void MemoryManualRateScoreStoreFindsWhatItSaved()
    {
        var store = ManualRateScoreStore.NewMemoryManualRateScoreStore();
        var item = ManualRateScore.NewManualRateScore("code-861", "name-357");
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
        MemoryManualRateScoreStoreFindsWhatItSaved();
    }
}
