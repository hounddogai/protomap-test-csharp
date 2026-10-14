namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingTrainingDueLedgerStoreChecks
{
    public static void MemoryRollingTrainingDueLedgerStoreFindsWhatItSaved()
    {
        var store = RollingTrainingDueLedgerStore.NewMemoryRollingTrainingDueLedgerStore();
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("code-937", "label-103");
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
        MemoryRollingTrainingDueLedgerStoreFindsWhatItSaved();
    }
}
