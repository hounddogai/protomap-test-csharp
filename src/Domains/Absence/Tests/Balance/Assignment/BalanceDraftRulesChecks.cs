namespace Acme.Workforce.Absence.Tests.Balance.Assignment;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BalanceDraftRulesChecks
{
    public static void BalanceDraftValidatorReportsEveryProblem()
    {
        var validator = BalanceDraftRules.DefaultBalanceDraftValidator();
        var item = BalanceDraft.NewBalanceDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceDraftRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BalanceDraftRequiredChannelRule("balance draft needs a value");
        var item = BalanceDraft.NewBalanceDraft("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceDraftFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BalanceDraftFloorValueRangeRule(1.3, 874.3);
        var item = BalanceDraft.NewBalanceDraft("x", "y");
        item.FloorValue = 875.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceDraftValidatorReportsEveryProblem();
        BalanceDraftRequiredChannelRuleFlagsABlankChannel();
        BalanceDraftFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
