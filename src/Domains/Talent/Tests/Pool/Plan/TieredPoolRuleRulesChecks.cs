namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TieredPoolRuleRulesChecks
{
    public static void TieredPoolRuleValidatorReportsEveryProblem()
    {
        var validator = TieredPoolRuleRules.DefaultTieredPoolRuleValidator();
        var item = TieredPoolRule.NewTieredPoolRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPoolRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredPoolRuleLabelLengthRule(86400);
        var item = TieredPoolRule.NewTieredPoolRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPoolRuleCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TieredPoolRuleCeilingRangeRule(4.0, 3563.5);
        var item = TieredPoolRule.NewTieredPoolRule("x", "y");
        item.Ceiling = 3565.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPoolRuleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TieredPoolRuleUpdatedAtRangeRule(64, 80);
        var item = TieredPoolRule.NewTieredPoolRule("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPoolRuleRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TieredPoolRuleRequiredDepartmentRule("tiered pool rule needs this field");
        var item = TieredPoolRule.NewTieredPoolRule("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPoolRuleRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new TieredPoolRuleRequiredChannelRule("tiered pool rule needs a value");
        var item = TieredPoolRule.NewTieredPoolRule("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPoolRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPoolRuleCodeLengthRule(80);
        var item = TieredPoolRule.NewTieredPoolRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPoolRuleValidatorReportsEveryProblem();
        TieredPoolRuleLabelLengthRuleFlagsALongLabel();
        TieredPoolRuleCeilingRangeRuleFlagsValuesAboveCeiling();
        TieredPoolRuleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TieredPoolRuleRequiredDepartmentRuleFlagsABlankDepartment();
        TieredPoolRuleRequiredChannelRuleFlagsABlankChannel();
        TieredPoolRuleCodeLengthRuleFlagsALongCode();
    }
}
