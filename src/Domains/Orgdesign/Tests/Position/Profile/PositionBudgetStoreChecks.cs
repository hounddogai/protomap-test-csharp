namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionBudgetStoreChecks
{
    public static void MemoryPositionBudgetStoreFindsWhatItSaved()
    {
        var store = PositionBudgetStore.NewMemoryPositionBudgetStore();
        var item = PositionBudget.NewPositionBudget("code-689", "name-334");
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
        MemoryPositionBudgetStoreFindsWhatItSaved();
    }
}
