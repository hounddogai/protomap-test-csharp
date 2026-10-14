namespace Acme.Workforce.Workplace.Tests.Desk.Milestone;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class DeskMilestoneStoreChecks
{
    public static void MemoryDeskMilestoneStoreFindsWhatItSaved()
    {
        var store = DeskMilestoneStore.NewMemoryDeskMilestoneStore();
        var item = DeskMilestone.NewDeskMilestone("code-644", "name-322");
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
        MemoryDeskMilestoneStoreFindsWhatItSaved();
    }
}
