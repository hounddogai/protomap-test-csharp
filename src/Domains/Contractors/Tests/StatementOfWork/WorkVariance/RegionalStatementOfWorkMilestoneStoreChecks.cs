namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkMilestoneStoreChecks
{
    public static void MemoryRegionalStatementOfWorkMilestoneStoreFindsWhatItSaved()
    {
        var store = RegionalStatementOfWorkMilestoneStore.NewMemoryRegionalStatementOfWorkMilestoneStore();
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("code-395", "title-864");
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
        MemoryRegionalStatementOfWorkMilestoneStoreFindsWhatItSaved();
    }
}
