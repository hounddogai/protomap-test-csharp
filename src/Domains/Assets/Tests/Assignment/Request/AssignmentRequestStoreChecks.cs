namespace Acme.Workforce.Assets.Tests.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssignmentRequestStoreChecks
{
    public static void MemoryAssignmentRequestStoreFindsWhatItSaved()
    {
        var store = AssignmentRequestStore.NewMemoryAssignmentRequestStore();
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
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
        MemoryAssignmentRequestStoreFindsWhatItSaved();
    }
}
