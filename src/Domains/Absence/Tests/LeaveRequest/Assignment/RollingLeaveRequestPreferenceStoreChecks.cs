namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RollingLeaveRequestPreferenceStoreChecks
{
    public static void MemoryRollingLeaveRequestPreferenceStoreFindsWhatItSaved()
    {
        var store = RollingLeaveRequestPreferenceStore.NewMemoryRollingLeaveRequestPreferenceStore();
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
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
        MemoryRollingLeaveRequestPreferenceStoreFindsWhatItSaved();
    }
}
