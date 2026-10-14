namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedSkillProfileForecastStoreChecks
{
    public static void MemoryCappedSkillProfileForecastStoreFindsWhatItSaved()
    {
        var store = CappedSkillProfileForecastStore.NewMemoryCappedSkillProfileForecastStore();
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("code-533", "label-88");
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
        MemoryCappedSkillProfileForecastStoreFindsWhatItSaved();
    }
}
