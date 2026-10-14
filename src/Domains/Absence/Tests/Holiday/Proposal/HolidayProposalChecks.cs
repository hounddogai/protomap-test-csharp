namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayProposalChecks
{
    public static void HolidayProposalStartsInOpen()
    {
        var item = HolidayProposal.NewHolidayProposal("code-306", "title-404");
        Check.Equal(item.Status, HolidayProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayProposalMovesToResolved()
    {
        var item = HolidayProposal.NewHolidayProposal("code-306", "title-404");
        try
        {
            item.MoveTo(HolidayProposalStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayProposalStatus.Resolved, "status");
    }

    public static void HolidayProposalRejectsMovesFromArchived()
    {
        var item = HolidayProposal.NewHolidayProposal("code-306", "title-404");
        item.Status = HolidayProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayProposalStartsInOpen();
        HolidayProposalMovesToResolved();
        HolidayProposalRejectsMovesFromArchived();
    }
}
