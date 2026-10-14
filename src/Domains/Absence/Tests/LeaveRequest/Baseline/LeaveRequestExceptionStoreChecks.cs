namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestExceptionStoreChecks
{
    public static void MemoryLeaveRequestExceptionStoreFindsWhatItSaved()
    {
        var store = LeaveRequestExceptionStore.NewMemoryLeaveRequestExceptionStore();
        var item = LeaveRequestException.NewLeaveRequestException("code-286", "label-81");
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
        MemoryLeaveRequestExceptionStoreFindsWhatItSaved();
    }
}
