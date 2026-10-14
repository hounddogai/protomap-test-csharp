namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowStoreChecks
{
    public static void MemoryOffboardingWindowStoreFindsWhatItSaved()
    {
        var store = OffboardingWindowStore.NewMemoryOffboardingWindowStore();
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
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
        MemoryOffboardingWindowStoreFindsWhatItSaved();
    }
}
