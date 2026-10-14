namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyTimesheetQuotaChecks
{
    public static void LegacyTimesheetQuotaStartsInCreated()
    {
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("code-873", "title-610");
        Check.Equal(item.Status, LegacyTimesheetQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTimesheetQuotaMovesToValidated()
    {
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("code-873", "title-610");
        try
        {
            item.MoveTo(LegacyTimesheetQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTimesheetQuotaStatus.Validated, "status");
    }

    public static void LegacyTimesheetQuotaRejectsMovesFromRetired()
    {
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("code-873", "title-610");
        item.Status = LegacyTimesheetQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyTimesheetQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyTimesheetQuotaStartsInCreated();
        LegacyTimesheetQuotaMovesToValidated();
        LegacyTimesheetQuotaRejectsMovesFromRetired();
    }
}
