namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyNoticeChecks
{
    public static void PolicyNoticeStartsInOpen()
    {
        var item = PolicyNotice.NewPolicyNotice("code-229", "name-318");
        Check.Equal(item.Status, PolicyNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyNoticeMovesToResolved()
    {
        var item = PolicyNotice.NewPolicyNotice("code-229", "name-318");
        try
        {
            item.MoveTo(PolicyNoticeStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyNoticeStatus.Resolved, "status");
    }

    public static void PolicyNoticeRejectsMovesFromArchived()
    {
        var item = PolicyNotice.NewPolicyNotice("code-229", "name-318");
        item.Status = PolicyNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyNoticeStartsInOpen();
        PolicyNoticeMovesToResolved();
        PolicyNoticeRejectsMovesFromArchived();
    }
}
