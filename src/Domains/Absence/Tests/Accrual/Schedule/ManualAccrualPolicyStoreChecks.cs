namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualAccrualPolicyStoreChecks
{
    public static void MemoryManualAccrualPolicyStoreFindsWhatItSaved()
    {
        var store = ManualAccrualPolicyStore.NewMemoryManualAccrualPolicyStore();
        var item = ManualAccrualPolicy.NewManualAccrualPolicy("code-193", "name-557");
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
        MemoryManualAccrualPolicyStoreFindsWhatItSaved();
    }
}
