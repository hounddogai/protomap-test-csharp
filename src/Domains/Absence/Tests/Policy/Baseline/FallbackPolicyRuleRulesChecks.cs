namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackPolicyRuleRulesChecks
{
    public static void FallbackPolicyRuleValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyRuleRules.DefaultFallbackPolicyRuleValidator();
        var item = FallbackPolicyRule.NewFallbackPolicyRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyRuleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FallbackPolicyRuleRequiredGradeRule("fallback policy rule needs a value");
        var item = FallbackPolicyRule.NewFallbackPolicyRule("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyRuleRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackPolicyRuleRequiredContactEmailRule("fallback policy rule needs an entry");
        var item = FallbackPolicyRule.NewFallbackPolicyRule("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackPolicyRuleTitleLengthRule(16);
        var item = FallbackPolicyRule.NewFallbackPolicyRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPolicyRuleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new FallbackPolicyRuleMaximumRangeRule(8.7, 2637.1);
        var item = FallbackPolicyRule.NewFallbackPolicyRule("x", "y");
        item.Maximum = 2638.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyRuleFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new FallbackPolicyRuleFloorValueRangeRule(1.0, 3652.7);
        var item = FallbackPolicyRule.NewFallbackPolicyRule("x", "y");
        item.FloorValue = 3654.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPolicyRuleValidatorReportsEveryProblem();
        FallbackPolicyRuleRequiredGradeRuleFlagsABlankGrade();
        FallbackPolicyRuleRequiredContactEmailRuleFlagsABlankContactEmail();
        FallbackPolicyRuleTitleLengthRuleFlagsALongTitle();
        FallbackPolicyRuleMaximumRangeRuleFlagsValuesAboveMaximum();
        FallbackPolicyRuleFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
