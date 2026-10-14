namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceCheckpointStoreChecks
{
    public static void MemoryLenientBalanceCheckpointStoreFindsWhatItSaved()
    {
        var store = LenientBalanceCheckpointStore.NewMemoryLenientBalanceCheckpointStore();
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("code-728", "title-794");
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
        MemoryLenientBalanceCheckpointStoreFindsWhatItSaved();
    }
}
