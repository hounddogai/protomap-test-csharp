namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredAssignmentAgreementStoreChecks
{
    public static void MemoryTieredAssignmentAgreementStoreFindsWhatItSaved()
    {
        var store = TieredAssignmentAgreementStore.NewMemoryTieredAssignmentAgreementStore();
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("code-132", "label-660");
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
        MemoryTieredAssignmentAgreementStoreFindsWhatItSaved();
    }
}
