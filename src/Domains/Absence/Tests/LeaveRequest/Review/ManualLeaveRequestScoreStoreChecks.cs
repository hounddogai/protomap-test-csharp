namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreStoreChecks
{
    public static void MemoryManualLeaveRequestScoreStoreFindsWhatItSaved()
    {
        var store = ManualLeaveRequestScoreStore.NewMemoryManualLeaveRequestScoreStore();
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
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
        MemoryManualLeaveRequestScoreStoreFindsWhatItSaved();
    }
}
