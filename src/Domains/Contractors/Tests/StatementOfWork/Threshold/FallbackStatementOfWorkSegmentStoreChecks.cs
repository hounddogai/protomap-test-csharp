namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkSegmentStoreChecks
{
    public static void MemoryFallbackStatementOfWorkSegmentStoreFindsWhatItSaved()
    {
        var store = FallbackStatementOfWorkSegmentStore.NewMemoryFallbackStatementOfWorkSegmentStore();
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
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
        MemoryFallbackStatementOfWorkSegmentStoreFindsWhatItSaved();
    }
}
