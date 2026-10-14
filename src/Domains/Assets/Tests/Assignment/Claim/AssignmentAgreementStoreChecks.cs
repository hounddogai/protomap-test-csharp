namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementStoreChecks
{
    public static void MemoryAssignmentAgreementStoreFindsWhatItSaved()
    {
        var store = AssignmentAgreementStore.NewMemoryAssignmentAgreementStore();
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
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
        MemoryAssignmentAgreementStoreFindsWhatItSaved();
    }
}
