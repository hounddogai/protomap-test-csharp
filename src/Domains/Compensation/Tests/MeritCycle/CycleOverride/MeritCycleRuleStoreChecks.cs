namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleRuleStoreChecks
{
    public static void MemoryMeritCycleRuleStoreFindsWhatItSaved()
    {
        var store = MeritCycleRuleStore.NewMemoryMeritCycleRuleStore();
        var item = MeritCycleRule.NewMeritCycleRule("code-410", "title-395");
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
        MemoryMeritCycleRuleStoreFindsWhatItSaved();
    }
}
