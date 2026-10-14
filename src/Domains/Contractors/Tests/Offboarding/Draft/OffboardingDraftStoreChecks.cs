namespace Acme.Workforce.Contractors.Tests.Offboarding.Draft;

using Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OffboardingDraftStoreChecks
{
    public static void MemoryOffboardingDraftStoreFindsWhatItSaved()
    {
        var store = OffboardingDraftStore.NewMemoryOffboardingDraftStore();
        var item = OffboardingDraft.NewOffboardingDraft("code-559", "name-550");
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
        MemoryOffboardingDraftStoreFindsWhatItSaved();
    }
}
