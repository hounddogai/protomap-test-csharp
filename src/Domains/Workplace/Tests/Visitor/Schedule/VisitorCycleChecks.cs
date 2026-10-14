namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorCycleChecks
{
    public static void VisitorCycleStartsInIdle()
    {
        var item = VisitorCycle.NewVisitorCycle("code-321", "name-272");
        Check.Equal(item.Status, VisitorCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorCycleMovesToWarming()
    {
        var item = VisitorCycle.NewVisitorCycle("code-321", "name-272");
        try
        {
            item.MoveTo(VisitorCycleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorCycleStatus.Warming, "status");
    }

    public static void VisitorCycleRejectsMovesFromStopped()
    {
        var item = VisitorCycle.NewVisitorCycle("code-321", "name-272");
        item.Status = VisitorCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorCycleStartsInIdle();
        VisitorCycleMovesToWarming();
        VisitorCycleRejectsMovesFromStopped();
    }
}
