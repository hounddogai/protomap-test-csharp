namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultDeskPolicyChecks
{
    public static void DefaultDeskPolicyStartsInPlanned()
    {
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
        Check.Equal(item.Status, DefaultDeskPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultDeskPolicyMovesToCompleted()
    {
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
        try
        {
            item.MoveTo(DefaultDeskPolicyStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultDeskPolicyStatus.Completed, "status");
    }

    public static void DefaultDeskPolicyRejectsMovesFromCancelled()
    {
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
        item.Status = DefaultDeskPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultDeskPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultDeskPolicyStartsInPlanned();
        DefaultDeskPolicyMovesToCompleted();
        DefaultDeskPolicyRejectsMovesFromCancelled();
    }
}
