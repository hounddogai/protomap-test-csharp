namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TrainingDueAgreementStoreChecks
{
    public static void MemoryTrainingDueAgreementStoreFindsWhatItSaved()
    {
        var store = TrainingDueAgreementStore.NewMemoryTrainingDueAgreementStore();
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
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
        MemoryTrainingDueAgreementStoreFindsWhatItSaved();
    }
}
