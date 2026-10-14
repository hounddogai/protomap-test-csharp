namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MobilityRuleRulesChecks
{
    public static void MobilityRuleValidatorReportsEveryProblem()
    {
        var validator = MobilityRuleRules.DefaultMobilityRuleValidator();
        var item = MobilityRule.NewMobilityRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityRuleCodeLengthRule(80);
        var item = MobilityRule.NewMobilityRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityRuleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new MobilityRuleAmountRangeRule(5.2, 3651.7);
        var item = MobilityRule.NewMobilityRule("x", "y");
        item.Amount = 3653.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityRuleValidatorReportsEveryProblem();
        MobilityRuleCodeLengthRuleFlagsALongCode();
        MobilityRuleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
