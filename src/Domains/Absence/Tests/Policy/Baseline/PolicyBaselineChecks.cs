namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PolicyBaselineChecks
{
    public static void PolicyBaselineStartsInPlanned()
    {
        var item = PolicyBaseline.NewPolicyBaseline("code-772", "name-559");
        Check.Equal(item.Status, PolicyBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyBaselineMovesToCompleted()
    {
        var item = PolicyBaseline.NewPolicyBaseline("code-772", "name-559");
        try
        {
            item.MoveTo(PolicyBaselineStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyBaselineStatus.Completed, "status");
    }

    public static void PolicyBaselineRejectsMovesFromCancelled()
    {
        var item = PolicyBaseline.NewPolicyBaseline("code-772", "name-559");
        item.Status = PolicyBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyBaselineStartsInPlanned();
        PolicyBaselineMovesToCompleted();
        PolicyBaselineRejectsMovesFromCancelled();
    }
}
