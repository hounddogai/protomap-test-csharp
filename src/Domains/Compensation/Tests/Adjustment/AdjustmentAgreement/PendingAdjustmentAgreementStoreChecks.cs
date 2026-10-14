namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class PendingAdjustmentAgreementStoreChecks
{
    public static void MemoryPendingAdjustmentAgreementStoreFindsWhatItSaved()
    {
        var store = PendingAdjustmentAgreementStore.NewMemoryPendingAdjustmentAgreementStore();
        var item = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement("code-376", "title-670");
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
        MemoryPendingAdjustmentAgreementStoreFindsWhatItSaved();
    }
}
