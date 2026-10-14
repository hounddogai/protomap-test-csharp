namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledRetroPayScoreStoreChecks
{
    public static void MemoryScheduledRetroPayScoreStoreFindsWhatItSaved()
    {
        var store = ScheduledRetroPayScoreStore.NewMemoryScheduledRetroPayScoreStore();
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
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
        MemoryScheduledRetroPayScoreStoreFindsWhatItSaved();
    }
}
