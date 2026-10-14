namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopMilestoneStoreChecks
{
    public static void MemoryLaptopMilestoneStoreFindsWhatItSaved()
    {
        var store = LaptopMilestoneStore.NewMemoryLaptopMilestoneStore();
        var item = LaptopMilestone.NewLaptopMilestone("code-794", "title-76");
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
        MemoryLaptopMilestoneStoreFindsWhatItSaved();
    }
}
