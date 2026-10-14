namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationProposalStoreChecks
{
    public static void MemoryCertificationProposalStoreFindsWhatItSaved()
    {
        var store = CertificationProposalStore.NewMemoryCertificationProposalStore();
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
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
        MemoryCertificationProposalStoreFindsWhatItSaved();
    }
}
