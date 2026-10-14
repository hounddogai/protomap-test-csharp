namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryTimesheetPlanChecks
{
    public static void PrimaryTimesheetPlanStartsInCreated()
    {
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("code-861", "name-569");
        Check.Equal(item.Status, PrimaryTimesheetPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryTimesheetPlanMovesToValidated()
    {
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("code-861", "name-569");
        try
        {
            item.MoveTo(PrimaryTimesheetPlanStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryTimesheetPlanStatus.Validated, "status");
    }

    public static void PrimaryTimesheetPlanRejectsMovesFromRetired()
    {
        var item = PrimaryTimesheetPlan.NewPrimaryTimesheetPlan("code-861", "name-569");
        item.Status = PrimaryTimesheetPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryTimesheetPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryTimesheetPlanStartsInCreated();
        PrimaryTimesheetPlanMovesToValidated();
        PrimaryTimesheetPlanRejectsMovesFromRetired();
    }
}
