namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class OffboardingAssignmentStoreChecks
{
    public static void MemoryOffboardingAssignmentStoreFindsWhatItSaved()
    {
        var store = OffboardingAssignmentStore.NewMemoryOffboardingAssignmentStore();
        var item = OffboardingAssignment.NewOffboardingAssignment("code-897", "label-188");
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
        MemoryOffboardingAssignmentStoreFindsWhatItSaved();
    }
}
