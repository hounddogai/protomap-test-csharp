namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultDeskPolicyStoreChecks
{
    public static void MemoryDefaultDeskPolicyStoreFindsWhatItSaved()
    {
        var store = DefaultDeskPolicyStore.NewMemoryDefaultDeskPolicyStore();
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
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
        MemoryDefaultDeskPolicyStoreFindsWhatItSaved();
    }
}
