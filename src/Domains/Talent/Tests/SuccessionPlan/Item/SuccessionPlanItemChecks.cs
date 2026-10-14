namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SuccessionPlanItemChecks
{
    public static void SuccessionPlanItemStartsInIdle()
    {
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
        Check.Equal(item.Status, SuccessionPlanItemStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanItemMovesToWarming()
    {
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
        try
        {
            item.MoveTo(SuccessionPlanItemStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanItemStatus.Warming, "status");
    }

    public static void SuccessionPlanItemRejectsMovesFromStopped()
    {
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
        item.Status = SuccessionPlanItemStatus.Stopped;
        Check.Fails(() => item.MoveTo(SuccessionPlanItemStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanItemStartsInIdle();
        SuccessionPlanItemMovesToWarming();
        SuccessionPlanItemRejectsMovesFromStopped();
    }
}
