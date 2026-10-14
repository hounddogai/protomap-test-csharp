namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionPolicyStoreChecks
{
    public static void MemoryPositionPolicyStoreFindsWhatItSaved()
    {
        var store = PositionPolicyStore.NewMemoryPositionPolicyStore();
        var item = PositionPolicy.NewPositionPolicy("code-840", "label-164");
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
        MemoryPositionPolicyStoreFindsWhatItSaved();
    }
}
