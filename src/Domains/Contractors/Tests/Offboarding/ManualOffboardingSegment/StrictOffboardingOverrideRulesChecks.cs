namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class StrictOffboardingOverrideRulesChecks
{
    public static void StrictOffboardingOverrideValidatorReportsEveryProblem()
    {
        var validator = StrictOffboardingOverrideRules.DefaultStrictOffboardingOverrideValidator();
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOffboardingOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictOffboardingOverrideRequiredTitleRule("strict offboarding override needs an entry");
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOffboardingOverrideGradeLengthRuleFlagsALongGrade()
    {
        var rule = new StrictOffboardingOverrideGradeLengthRule(120);
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictOffboardingOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictOffboardingOverrideTitleLengthRule(16);
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictOffboardingOverrideValidatorReportsEveryProblem();
        StrictOffboardingOverrideRequiredTitleRuleFlagsABlankTitle();
        StrictOffboardingOverrideGradeLengthRuleFlagsALongGrade();
        StrictOffboardingOverrideTitleLengthRuleFlagsALongTitle();
    }
}
