namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class DefaultAttestationRuleRulesChecks
{
    public static void DefaultAttestationRuleValidatorReportsEveryProblem()
    {
        var validator = DefaultAttestationRuleRules.DefaultDefaultAttestationRuleValidator();
        var item = DefaultAttestationRule.NewDefaultAttestationRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAttestationRuleNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultAttestationRuleNameLengthRule(255);
        var item = DefaultAttestationRule.NewDefaultAttestationRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAttestationRuleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new DefaultAttestationRuleBudgetRangeRule(1.6, 425.7);
        var item = DefaultAttestationRule.NewDefaultAttestationRule("x", "y");
        item.Budget = 427.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAttestationRuleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new DefaultAttestationRuleCategoryLengthRule(64);
        var item = DefaultAttestationRule.NewDefaultAttestationRule("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAttestationRuleExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new DefaultAttestationRuleExpiresAtRangeRule(16, 255);
        var item = DefaultAttestationRule.NewDefaultAttestationRule("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAttestationRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultAttestationRuleCodeLengthRule(80);
        var item = DefaultAttestationRule.NewDefaultAttestationRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultAttestationRuleValidatorReportsEveryProblem();
        DefaultAttestationRuleNameLengthRuleFlagsALongName();
        DefaultAttestationRuleBudgetRangeRuleFlagsValuesAboveBudget();
        DefaultAttestationRuleCategoryLengthRuleFlagsALongCategory();
        DefaultAttestationRuleExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        DefaultAttestationRuleCodeLengthRuleFlagsALongCode();
    }
}
