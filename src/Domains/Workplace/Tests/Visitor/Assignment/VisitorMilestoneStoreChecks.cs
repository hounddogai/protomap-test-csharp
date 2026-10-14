namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorMilestoneStoreChecks
{
    public static void MemoryVisitorMilestoneStoreFindsWhatItSaved()
    {
        var store = VisitorMilestoneStore.NewMemoryVisitorMilestoneStore();
        var item = VisitorMilestone.NewVisitorMilestone("code-514", "title-147");
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
        MemoryVisitorMilestoneStoreFindsWhatItSaved();
    }
}
