namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalVisitorAdjustmentChecks
{
    public static void GlobalVisitorAdjustmentStartsInCreated()
    {
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("code-835", "label-68");
        Check.Equal(item.Status, GlobalVisitorAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalVisitorAdjustmentMovesToValidated()
    {
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("code-835", "label-68");
        try
        {
            item.MoveTo(GlobalVisitorAdjustmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalVisitorAdjustmentStatus.Validated, "status");
    }

    public static void GlobalVisitorAdjustmentRejectsMovesFromRetired()
    {
        var item = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment("code-835", "label-68");
        item.Status = GlobalVisitorAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalVisitorAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalVisitorAdjustmentStartsInCreated();
        GlobalVisitorAdjustmentMovesToValidated();
        GlobalVisitorAdjustmentRejectsMovesFromRetired();
    }
}
