namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalOffboardingNoticeStoreChecks
{
    public static void MemoryRegionalOffboardingNoticeStoreFindsWhatItSaved()
    {
        var store = RegionalOffboardingNoticeStore.NewMemoryRegionalOffboardingNoticeStore();
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("code-392", "label-491");
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
        MemoryRegionalOffboardingNoticeStoreFindsWhatItSaved();
    }
}
