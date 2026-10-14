namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class StrictCareerPathProposalStoreChecks
{
    public static void MemoryStrictCareerPathProposalStoreFindsWhatItSaved()
    {
        var store = StrictCareerPathProposalStore.NewMemoryStrictCareerPathProposalStore();
        var item = StrictCareerPathProposal.NewStrictCareerPathProposal("code-178", "title-278");
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
        MemoryStrictCareerPathProposalStoreFindsWhatItSaved();
    }
}
