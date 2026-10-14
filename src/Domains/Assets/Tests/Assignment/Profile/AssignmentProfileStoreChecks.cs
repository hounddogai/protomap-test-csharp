namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssignmentProfileStoreChecks
{
    public static void MemoryAssignmentProfileStoreFindsWhatItSaved()
    {
        var store = AssignmentProfileStore.NewMemoryAssignmentProfileStore();
        var item = AssignmentProfile.NewAssignmentProfile("code-542", "name-81");
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
        MemoryAssignmentProfileStoreFindsWhatItSaved();
    }
}
