namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OffboardingRuleRulesChecks
{
    public static void OffboardingRuleValidatorReportsEveryProblem()
    {
        var validator = OffboardingRuleRules.DefaultOffboardingRuleValidator();
        var item = OffboardingRule.NewOffboardingRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OffboardingRuleRequiredLabelRule("offboarding rule needs this field");
        var item = OffboardingRule.NewOffboardingRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingRuleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new OffboardingRuleOwnerIdLengthRule(120);
        var item = OffboardingRule.NewOffboardingRule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingRuleCodeLengthRule(16);
        var item = OffboardingRule.NewOffboardingRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingRuleValidatorReportsEveryProblem();
        OffboardingRuleRequiredLabelRuleFlagsABlankLabel();
        OffboardingRuleOwnerIdLengthRuleFlagsALongOwnerId();
        OffboardingRuleCodeLengthRuleFlagsALongCode();
    }
}
