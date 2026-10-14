namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultReturnPlanAuditChecks
{
    public static void DefaultReturnPlanAuditStartsInRequested()
    {
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("code-677", "title-921");
        Check.Equal(item.Status, DefaultReturnPlanAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultReturnPlanAuditMovesToGranted()
    {
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("code-677", "title-921");
        try
        {
            item.MoveTo(DefaultReturnPlanAuditStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultReturnPlanAuditStatus.Granted, "status");
    }

    public static void DefaultReturnPlanAuditRejectsMovesFromExpired()
    {
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("code-677", "title-921");
        item.Status = DefaultReturnPlanAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultReturnPlanAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultReturnPlanAuditStartsInRequested();
        DefaultReturnPlanAuditMovesToGranted();
        DefaultReturnPlanAuditRejectsMovesFromExpired();
    }
}
