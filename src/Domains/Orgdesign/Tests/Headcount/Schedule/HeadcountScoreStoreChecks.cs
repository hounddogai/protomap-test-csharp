namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountScoreStoreChecks
{
    public static void MemoryHeadcountScoreStoreFindsWhatItSaved()
    {
        var store = HeadcountScoreStore.NewMemoryHeadcountScoreStore();
        var item = HeadcountScore.NewHeadcountScore("code-598", "title-65");
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
        MemoryHeadcountScoreStoreFindsWhatItSaved();
    }
}
