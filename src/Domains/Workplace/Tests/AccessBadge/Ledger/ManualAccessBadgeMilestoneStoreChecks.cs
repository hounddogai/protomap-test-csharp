namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualAccessBadgeMilestoneStoreChecks
{
    public static void MemoryManualAccessBadgeMilestoneStoreFindsWhatItSaved()
    {
        var store = ManualAccessBadgeMilestoneStore.NewMemoryManualAccessBadgeMilestoneStore();
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("code-226", "title-148");
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
        MemoryManualAccessBadgeMilestoneStoreFindsWhatItSaved();
    }
}
