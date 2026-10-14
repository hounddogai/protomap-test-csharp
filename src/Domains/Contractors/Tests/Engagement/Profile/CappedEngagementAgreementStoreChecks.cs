namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedEngagementAgreementStoreChecks
{
    public static void MemoryCappedEngagementAgreementStoreFindsWhatItSaved()
    {
        var store = CappedEngagementAgreementStore.NewMemoryCappedEngagementAgreementStore();
        var item = CappedEngagementAgreement.NewCappedEngagementAgreement("code-439", "name-342");
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
        MemoryCappedEngagementAgreementStoreFindsWhatItSaved();
    }
}
