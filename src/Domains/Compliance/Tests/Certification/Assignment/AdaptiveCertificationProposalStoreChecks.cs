namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptiveCertificationProposalStoreChecks
{
    public static void MemoryAdaptiveCertificationProposalStoreFindsWhatItSaved()
    {
        var store = AdaptiveCertificationProposalStore.NewMemoryAdaptiveCertificationProposalStore();
        var item = AdaptiveCertificationProposal.NewAdaptiveCertificationProposal("code-290", "label-777");
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
        MemoryAdaptiveCertificationProposalStoreFindsWhatItSaved();
    }
}
