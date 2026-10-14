namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RegionalTimesheetReviewChecks
{
    public static void RegionalTimesheetReviewStartsInCreated()
    {
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("code-692", "label-989");
        Check.Equal(item.Status, RegionalTimesheetReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTimesheetReviewMovesToPublished()
    {
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("code-692", "label-989");
        try
        {
            item.MoveTo(RegionalTimesheetReviewStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTimesheetReviewStatus.Published, "status");
    }

    public static void RegionalTimesheetReviewRejectsMovesFromRetired()
    {
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("code-692", "label-989");
        item.Status = RegionalTimesheetReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalTimesheetReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalTimesheetReviewStartsInCreated();
        RegionalTimesheetReviewMovesToPublished();
        RegionalTimesheetReviewRejectsMovesFromRetired();
    }
}
