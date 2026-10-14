namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentClaimStoreChecks
{
    public static void MemoryAssignmentClaimStoreFindsWhatItSaved()
    {
        var store = AssignmentClaimStore.NewMemoryAssignmentClaimStore();
        var item = AssignmentClaim.NewAssignmentClaim("code-356", "name-308");
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
        MemoryAssignmentClaimStoreFindsWhatItSaved();
    }
}
