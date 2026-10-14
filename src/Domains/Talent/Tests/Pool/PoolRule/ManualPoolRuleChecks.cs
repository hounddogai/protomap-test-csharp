namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class ManualPoolRuleChecks
{
    public static void ManualPoolRuleStartsInCreated()
    {
        var item = ManualPoolRule.NewManualPoolRule("code-350", "name-673");
        Check.Equal(item.Status, ManualPoolRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPoolRuleMovesToValidated()
    {
        var item = ManualPoolRule.NewManualPoolRule("code-350", "name-673");
        try
        {
            item.MoveTo(ManualPoolRuleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPoolRuleStatus.Validated, "status");
    }

    public static void ManualPoolRuleRejectsMovesFromRetired()
    {
        var item = ManualPoolRule.NewManualPoolRule("code-350", "name-673");
        item.Status = ManualPoolRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualPoolRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualPoolRuleStartsInCreated();
        ManualPoolRuleMovesToValidated();
        ManualPoolRuleRejectsMovesFromRetired();
    }
}
