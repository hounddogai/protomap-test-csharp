namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RegionalAssignmentAgreementStoreChecks
{
    public static void MemoryRegionalAssignmentAgreementStoreFindsWhatItSaved()
    {
        var store = RegionalAssignmentAgreementStore.NewMemoryRegionalAssignmentAgreementStore();
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("code-639", "name-487");
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
        MemoryRegionalAssignmentAgreementStoreFindsWhatItSaved();
    }
}
