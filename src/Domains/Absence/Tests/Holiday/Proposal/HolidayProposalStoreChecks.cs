namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayProposalStoreChecks
{
    public static void MemoryHolidayProposalStoreFindsWhatItSaved()
    {
        var store = HolidayProposalStore.NewMemoryHolidayProposalStore();
        var item = HolidayProposal.NewHolidayProposal("code-306", "title-404");
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
        MemoryHolidayProposalStoreFindsWhatItSaved();
    }
}
