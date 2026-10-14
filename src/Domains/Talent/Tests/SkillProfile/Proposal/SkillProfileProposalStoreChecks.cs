namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileProposalStoreChecks
{
    public static void MemorySkillProfileProposalStoreFindsWhatItSaved()
    {
        var store = SkillProfileProposalStore.NewMemorySkillProfileProposalStore();
        var item = SkillProfileProposal.NewSkillProfileProposal("code-434", "label-269");
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
        MemorySkillProfileProposalStoreFindsWhatItSaved();
    }
}
