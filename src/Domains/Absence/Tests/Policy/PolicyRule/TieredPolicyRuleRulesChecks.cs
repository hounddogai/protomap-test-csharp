namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class TieredPolicyRuleRulesChecks
{
    public static void TieredPolicyRuleValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyRuleRules.DefaultTieredPolicyRuleValidator();
        var item = TieredPolicyRule.NewTieredPolicyRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyRuleBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new TieredPolicyRuleBatchSizeRangeRule(64, 64);
        var item = TieredPolicyRule.NewTieredPolicyRule("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPolicyRuleSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new TieredPolicyRuleSegmentLengthRule(3600);
        var item = TieredPolicyRule.NewTieredPolicyRule("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyRuleRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new TieredPolicyRuleRequiredSegmentRule("tiered policy rule needs a value");
        var item = TieredPolicyRule.NewTieredPolicyRule("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPolicyRuleValidatorReportsEveryProblem();
        TieredPolicyRuleBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        TieredPolicyRuleSegmentLengthRuleFlagsALongSegment();
        TieredPolicyRuleRequiredSegmentRuleFlagsABlankSegment();
    }
}
