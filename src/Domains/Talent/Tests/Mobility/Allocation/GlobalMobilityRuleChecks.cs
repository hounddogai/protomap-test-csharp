namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GlobalMobilityRuleChecks
{
    public static void GlobalMobilityRuleStartsInDraft()
    {
        var item = GlobalMobilityRule.NewGlobalMobilityRule("code-900", "title-815");
        Check.Equal(item.Status, GlobalMobilityRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalMobilityRuleMovesToPending()
    {
        var item = GlobalMobilityRule.NewGlobalMobilityRule("code-900", "title-815");
        try
        {
            item.MoveTo(GlobalMobilityRuleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalMobilityRuleStatus.Pending, "status");
    }

    public static void GlobalMobilityRuleRejectsMovesFromClosed()
    {
        var item = GlobalMobilityRule.NewGlobalMobilityRule("code-900", "title-815");
        item.Status = GlobalMobilityRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalMobilityRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalMobilityRuleStartsInDraft();
        GlobalMobilityRuleMovesToPending();
        GlobalMobilityRuleRejectsMovesFromClosed();
    }
}
