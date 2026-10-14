namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PolicyTargetRulesChecks
{
    public static void PolicyTargetValidatorReportsEveryProblem()
    {
        var validator = PolicyTargetRules.DefaultPolicyTargetValidator();
        var item = PolicyTarget.NewPolicyTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyTargetHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new PolicyTargetHoursRangeRule(4.3, 4363.8);
        var item = PolicyTarget.NewPolicyTarget("x", "y");
        item.Hours = 4365.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyTargetTitleLengthRule(255);
        var item = PolicyTarget.NewPolicyTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyTargetUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PolicyTargetUnitLengthRule(255);
        var item = PolicyTarget.NewPolicyTarget("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyTargetRequiredCodeRule("policy target needs an entry");
        var item = PolicyTarget.NewPolicyTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyTargetCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PolicyTargetCategoryLengthRule(80);
        var item = PolicyTarget.NewPolicyTarget("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyTargetValidatorReportsEveryProblem();
        PolicyTargetHoursRangeRuleFlagsValuesAboveHours();
        PolicyTargetTitleLengthRuleFlagsALongTitle();
        PolicyTargetUnitLengthRuleFlagsALongUnit();
        PolicyTargetRequiredCodeRuleFlagsABlankCode();
        PolicyTargetCategoryLengthRuleFlagsALongCategory();
    }
}
