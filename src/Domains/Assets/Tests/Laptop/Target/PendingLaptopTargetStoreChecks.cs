namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetStoreChecks
{
    public static void MemoryPendingLaptopTargetStoreFindsWhatItSaved()
    {
        var store = PendingLaptopTargetStore.NewMemoryPendingLaptopTargetStore();
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
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
        MemoryPendingLaptopTargetStoreFindsWhatItSaved();
    }
}
