namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredEngagementAgreementStoreChecks
{
    public static void MemoryTieredEngagementAgreementStoreFindsWhatItSaved()
    {
        var store = TieredEngagementAgreementStore.NewMemoryTieredEngagementAgreementStore();
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("code-279", "title-646");
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
        MemoryTieredEngagementAgreementStoreFindsWhatItSaved();
    }
}
