namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OffboardingAgreementStoreChecks
{
    public static void MemoryOffboardingAgreementStoreFindsWhatItSaved()
    {
        var store = OffboardingAgreementStore.NewMemoryOffboardingAgreementStore();
        var item = OffboardingAgreement.NewOffboardingAgreement("code-819", "label-714");
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
        MemoryOffboardingAgreementStoreFindsWhatItSaved();
    }
}
