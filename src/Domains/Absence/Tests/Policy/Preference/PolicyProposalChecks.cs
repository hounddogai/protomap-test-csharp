namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyProposalChecks
{
    public static void PolicyProposalStartsInNew()
    {
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
        Check.Equal(item.Status, PolicyProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyProposalMovesToRunning()
    {
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
        try
        {
            item.MoveTo(PolicyProposalStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyProposalStatus.Running, "status");
    }

    public static void PolicyProposalRejectsMovesFromFailed()
    {
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
        item.Status = PolicyProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(PolicyProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        PolicyProposalStartsInNew();
        PolicyProposalMovesToRunning();
        PolicyProposalRejectsMovesFromFailed();
    }
}
