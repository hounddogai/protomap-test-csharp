namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class DefaultSkillProfileForecastStoreChecks
{
    public static void MemoryDefaultSkillProfileForecastStoreFindsWhatItSaved()
    {
        var store = DefaultSkillProfileForecastStore.NewMemoryDefaultSkillProfileForecastStore();
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("code-933", "label-502");
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
        MemoryDefaultSkillProfileForecastStoreFindsWhatItSaved();
    }
}
