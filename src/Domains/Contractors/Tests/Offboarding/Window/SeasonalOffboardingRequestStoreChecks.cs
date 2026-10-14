namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalOffboardingRequestStoreChecks
{
    public static void MemorySeasonalOffboardingRequestStoreFindsWhatItSaved()
    {
        var store = SeasonalOffboardingRequestStore.NewMemorySeasonalOffboardingRequestStore();
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("code-441", "label-613");
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
        MemorySeasonalOffboardingRequestStoreFindsWhatItSaved();
    }
}
