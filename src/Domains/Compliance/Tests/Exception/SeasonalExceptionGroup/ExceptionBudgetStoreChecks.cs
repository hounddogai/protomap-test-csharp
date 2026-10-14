namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionBudgetStoreChecks
{
    public static void MemoryExceptionBudgetStoreFindsWhatItSaved()
    {
        var store = ExceptionBudgetStore.NewMemoryExceptionBudgetStore();
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
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
        MemoryExceptionBudgetStoreFindsWhatItSaved();
    }
}
