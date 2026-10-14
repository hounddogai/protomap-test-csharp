namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathTemplateStoreChecks
{
    public static void MemoryCareerPathTemplateStoreFindsWhatItSaved()
    {
        var store = CareerPathTemplateStore.NewMemoryCareerPathTemplateStore();
        var item = CareerPathTemplate.NewCareerPathTemplate("code-571", "name-880");
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
        MemoryCareerPathTemplateStoreFindsWhatItSaved();
    }
}
