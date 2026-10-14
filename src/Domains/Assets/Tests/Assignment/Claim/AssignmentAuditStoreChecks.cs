namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAuditStoreChecks
{
    public static void MemoryAssignmentAuditStoreFindsWhatItSaved()
    {
        var store = AssignmentAuditStore.NewMemoryAssignmentAuditStore();
        var item = AssignmentAudit.NewAssignmentAudit("code-696", "label-17");
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
        MemoryAssignmentAuditStoreFindsWhatItSaved();
    }
}
