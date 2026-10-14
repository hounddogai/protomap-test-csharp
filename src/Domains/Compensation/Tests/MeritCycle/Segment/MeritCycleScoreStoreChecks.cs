namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleScoreStoreChecks
{
    public static void MemoryMeritCycleScoreStoreFindsWhatItSaved()
    {
        var store = MeritCycleScoreStore.NewMemoryMeritCycleScoreStore();
        var item = MeritCycleScore.NewMeritCycleScore("code-739", "name-622");
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
        MemoryMeritCycleScoreStoreFindsWhatItSaved();
    }
}
