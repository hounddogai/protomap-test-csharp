namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkEstimateStoreChecks
{
    public static void MemoryStatementOfWorkEstimateStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkEstimateStore.NewMemoryStatementOfWorkEstimateStore();
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
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
        MemoryStatementOfWorkEstimateStoreFindsWhatItSaved();
    }
}
