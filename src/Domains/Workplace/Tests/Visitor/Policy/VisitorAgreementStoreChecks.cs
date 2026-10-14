namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementStoreChecks
{
    public static void MemoryVisitorAgreementStoreFindsWhatItSaved()
    {
        var store = VisitorAgreementStore.NewMemoryVisitorAgreementStore();
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
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
        MemoryVisitorAgreementStoreFindsWhatItSaved();
    }
}
