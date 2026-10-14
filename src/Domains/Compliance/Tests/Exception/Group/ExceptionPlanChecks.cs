namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionPlanChecks
{
    public static void ExceptionPlanStartsInRequested()
    {
        var item = ExceptionPlan.NewExceptionPlan("code-140", "label-622");
        Check.Equal(item.Status, ExceptionPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionPlanMovesToExpired()
    {
        var item = ExceptionPlan.NewExceptionPlan("code-140", "label-622");
        try
        {
            item.MoveTo(ExceptionPlanStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionPlanStatus.Expired, "status");
    }

    public static void ExceptionPlanRejectsMovesFromExpired()
    {
        var item = ExceptionPlan.NewExceptionPlan("code-140", "label-622");
        item.Status = ExceptionPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(ExceptionPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ExceptionPlanStartsInRequested();
        ExceptionPlanMovesToExpired();
        ExceptionPlanRejectsMovesFromExpired();
    }
}
