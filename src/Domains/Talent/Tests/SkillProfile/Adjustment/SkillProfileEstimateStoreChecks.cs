namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SkillProfileEstimateStoreChecks
{
    public static void MemorySkillProfileEstimateStoreFindsWhatItSaved()
    {
        var store = SkillProfileEstimateStore.NewMemorySkillProfileEstimateStore();
        var item = SkillProfileEstimate.NewSkillProfileEstimate("code-561", "label-31");
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
        MemorySkillProfileEstimateStoreFindsWhatItSaved();
    }
}
