namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAgreementStoreChecks
{
    public static void MemoryFallbackAccessBadgeAgreementStoreFindsWhatItSaved()
    {
        var store = FallbackAccessBadgeAgreementStore.NewMemoryFallbackAccessBadgeAgreementStore();
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("code-852", "title-277");
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
        MemoryFallbackAccessBadgeAgreementStoreFindsWhatItSaved();
    }
}
