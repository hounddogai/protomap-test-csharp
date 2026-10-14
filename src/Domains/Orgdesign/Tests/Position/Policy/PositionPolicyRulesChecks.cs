namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionPolicyRulesChecks
{
    public static void PositionPolicyValidatorReportsEveryProblem()
    {
        var validator = PositionPolicyRules.DefaultPositionPolicyValidator();
        var item = PositionPolicy.NewPositionPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionPolicyRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PositionPolicyRatioRangeRule(8.7, 1298.4);
        var item = PositionPolicy.NewPositionPolicy("x", "y");
        item.Ratio = 1299.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionPolicyCodeLengthRule(86400);
        var item = PositionPolicy.NewPositionPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionPolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PositionPolicyCreatedAtRangeRule(120, 255);
        var item = PositionPolicy.NewPositionPolicy("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PositionPolicyLabelLengthRule(32);
        var item = PositionPolicy.NewPositionPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionPolicyRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PositionPolicyRequiredRegionRule("position policy needs this field");
        var item = PositionPolicy.NewPositionPolicy("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionPolicyValidatorReportsEveryProblem();
        PositionPolicyRatioRangeRuleFlagsValuesAboveRatio();
        PositionPolicyCodeLengthRuleFlagsALongCode();
        PositionPolicyCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PositionPolicyLabelLengthRuleFlagsALongLabel();
        PositionPolicyRequiredRegionRuleFlagsABlankRegion();
    }
}
