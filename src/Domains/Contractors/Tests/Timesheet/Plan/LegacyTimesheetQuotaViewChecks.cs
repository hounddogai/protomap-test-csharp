namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyTimesheetQuotaViewChecks
{
    public static void LegacyTimesheetQuotaViewShowsTheStatusInCapitals()
    {
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("code-873", "title-610");
        var view = LegacyTimesheetQuotaView.ToLegacyTimesheetQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTimesheetQuotaView.ToLegacyTimesheetQuotaViews(new List<LegacyTimesheetQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTimesheetQuotaViewShowsTheStatusInCapitals();
    }
}
