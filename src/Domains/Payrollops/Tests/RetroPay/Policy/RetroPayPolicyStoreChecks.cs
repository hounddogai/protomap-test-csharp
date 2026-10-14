namespace Acme.Workforce.Payrollops.Tests.RetroPay.Policy;

using Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RetroPayPolicyStoreChecks
{
    public static void MemoryRetroPayPolicyStoreFindsWhatItSaved()
    {
        var store = RetroPayPolicyStore.NewMemoryRetroPayPolicyStore();
        var item = RetroPayPolicy.NewRetroPayPolicy("code-417", "name-898");
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
        MemoryRetroPayPolicyStoreFindsWhatItSaved();
    }
}
