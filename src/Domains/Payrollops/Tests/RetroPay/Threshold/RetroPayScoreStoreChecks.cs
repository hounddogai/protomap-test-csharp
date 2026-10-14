namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreStoreChecks
{
    public static void MemoryRetroPayScoreStoreFindsWhatItSaved()
    {
        var store = RetroPayScoreStore.NewMemoryRetroPayScoreStore();
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
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
        MemoryRetroPayScoreStoreFindsWhatItSaved();
    }
}
