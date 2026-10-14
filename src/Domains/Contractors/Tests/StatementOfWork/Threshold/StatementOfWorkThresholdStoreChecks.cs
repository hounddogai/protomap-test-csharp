namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkThresholdStoreChecks
{
    public static void MemoryStatementOfWorkThresholdStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkThresholdStore.NewMemoryStatementOfWorkThresholdStore();
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("code-413", "label-101");
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
        MemoryStatementOfWorkThresholdStoreFindsWhatItSaved();
    }
}
