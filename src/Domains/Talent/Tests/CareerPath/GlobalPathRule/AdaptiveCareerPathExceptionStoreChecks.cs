namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class AdaptiveCareerPathExceptionStoreChecks
{
    public static void MemoryAdaptiveCareerPathExceptionStoreFindsWhatItSaved()
    {
        var store = AdaptiveCareerPathExceptionStore.NewMemoryAdaptiveCareerPathExceptionStore();
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("code-163", "label-535");
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
        MemoryAdaptiveCareerPathExceptionStoreFindsWhatItSaved();
    }
}
