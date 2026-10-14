namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathBudgetStoreChecks
{
    public static void MemoryCareerPathBudgetStoreFindsWhatItSaved()
    {
        var store = CareerPathBudgetStore.NewMemoryCareerPathBudgetStore();
        var item = CareerPathBudget.NewCareerPathBudget("code-906", "name-726");
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
        MemoryCareerPathBudgetStoreFindsWhatItSaved();
    }
}
