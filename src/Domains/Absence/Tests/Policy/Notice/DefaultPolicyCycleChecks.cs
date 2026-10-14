namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class DefaultPolicyCycleChecks
{
    public static void DefaultPolicyCycleStartsInPlanned()
    {
        var item = DefaultPolicyCycle.NewDefaultPolicyCycle("code-618", "title-651");
        Check.Equal(item.Status, DefaultPolicyCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPolicyCycleMovesToScheduled()
    {
        var item = DefaultPolicyCycle.NewDefaultPolicyCycle("code-618", "title-651");
        try
        {
            item.MoveTo(DefaultPolicyCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPolicyCycleStatus.Scheduled, "status");
    }

    public static void DefaultPolicyCycleRejectsMovesFromCancelled()
    {
        var item = DefaultPolicyCycle.NewDefaultPolicyCycle("code-618", "title-651");
        item.Status = DefaultPolicyCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultPolicyCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultPolicyCycleStartsInPlanned();
        DefaultPolicyCycleMovesToScheduled();
        DefaultPolicyCycleRejectsMovesFromCancelled();
    }
}
