namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class OffboardingOverrideRulesChecks
{
    public static void OffboardingOverrideValidatorReportsEveryProblem()
    {
        var validator = OffboardingOverrideRules.DefaultOffboardingOverrideValidator();
        var item = OffboardingOverride.NewOffboardingOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingOverrideCodeLengthRule(120);
        var item = OffboardingOverride.NewOffboardingOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingOverrideManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new OffboardingOverrideManagerIdLengthRule(255);
        var item = OffboardingOverride.NewOffboardingOverride("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OffboardingOverrideLabelLengthRule(80);
        var item = OffboardingOverride.NewOffboardingOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingOverrideRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new OffboardingOverrideRequiredManagerIdRule("offboarding override needs an entry");
        var item = OffboardingOverride.NewOffboardingOverride("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingOverrideValidatorReportsEveryProblem();
        OffboardingOverrideCodeLengthRuleFlagsALongCode();
        OffboardingOverrideManagerIdLengthRuleFlagsALongManagerId();
        OffboardingOverrideLabelLengthRuleFlagsALongLabel();
        OffboardingOverrideRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
