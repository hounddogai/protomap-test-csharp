namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BalanceTemplateStoreChecks
{
    public static void MemoryBalanceTemplateStoreFindsWhatItSaved()
    {
        var store = BalanceTemplateStore.NewMemoryBalanceTemplateStore();
        var item = BalanceTemplate.NewBalanceTemplate("code-301", "name-924");
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
        MemoryBalanceTemplateStoreFindsWhatItSaved();
    }
}
