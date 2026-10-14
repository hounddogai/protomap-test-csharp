namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayMilestoneStoreChecks
{
    public static void MemoryRetroPayMilestoneStoreFindsWhatItSaved()
    {
        var store = RetroPayMilestoneStore.NewMemoryRetroPayMilestoneStore();
        var item = RetroPayMilestone.NewRetroPayMilestone("code-388", "label-926");
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
        MemoryRetroPayMilestoneStoreFindsWhatItSaved();
    }
}
