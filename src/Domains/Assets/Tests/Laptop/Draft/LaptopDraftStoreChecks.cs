namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopDraftStoreChecks
{
    public static void MemoryLaptopDraftStoreFindsWhatItSaved()
    {
        var store = LaptopDraftStore.NewMemoryLaptopDraftStore();
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
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
        MemoryLaptopDraftStoreFindsWhatItSaved();
    }
}
