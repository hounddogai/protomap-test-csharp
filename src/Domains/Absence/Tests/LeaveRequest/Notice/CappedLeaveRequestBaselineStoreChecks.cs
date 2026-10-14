namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedLeaveRequestBaselineStoreChecks
{
    public static void MemoryCappedLeaveRequestBaselineStoreFindsWhatItSaved()
    {
        var store = CappedLeaveRequestBaselineStore.NewMemoryCappedLeaveRequestBaselineStore();
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("code-560", "title-383");
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
        MemoryCappedLeaveRequestBaselineStoreFindsWhatItSaved();
    }
}
