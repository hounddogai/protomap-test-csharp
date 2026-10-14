namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ReorgRuleChecks
{
    public static void ReorgRuleStartsInCreated()
    {
        var item = ReorgRule.NewReorgRule("code-199", "name-253");
        Check.Equal(item.Status, ReorgRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgRuleMovesToValidated()
    {
        var item = ReorgRule.NewReorgRule("code-199", "name-253");
        try
        {
            item.MoveTo(ReorgRuleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgRuleStatus.Validated, "status");
    }

    public static void ReorgRuleRejectsMovesFromRetired()
    {
        var item = ReorgRule.NewReorgRule("code-199", "name-253");
        item.Status = ReorgRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(ReorgRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReorgRuleStartsInCreated();
        ReorgRuleMovesToValidated();
        ReorgRuleRejectsMovesFromRetired();
    }
}
