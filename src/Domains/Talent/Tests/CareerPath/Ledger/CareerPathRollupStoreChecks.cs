namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathRollupStoreChecks
{
    public static void MemoryCareerPathRollupStoreFindsWhatItSaved()
    {
        var store = CareerPathRollupStore.NewMemoryCareerPathRollupStore();
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
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
        MemoryCareerPathRollupStoreFindsWhatItSaved();
    }
}
