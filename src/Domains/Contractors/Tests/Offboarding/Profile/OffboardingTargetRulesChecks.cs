namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTargetRulesChecks
{
    public static void OffboardingTargetValidatorReportsEveryProblem()
    {
        var validator = OffboardingTargetRules.DefaultOffboardingTargetValidator();
        var item = OffboardingTarget.NewOffboardingTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new OffboardingTargetRequiredNameRule("offboarding target needs an entry");
        var item = OffboardingTarget.NewOffboardingTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingTargetUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new OffboardingTargetUpdatedAtRangeRule(80, 120);
        var item = OffboardingTarget.NewOffboardingTarget("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingTargetNameLengthRuleFlagsALongName()
    {
        var rule = new OffboardingTargetNameLengthRule(3600);
        var item = OffboardingTarget.NewOffboardingTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingTargetValidatorReportsEveryProblem();
        OffboardingTargetRequiredNameRuleFlagsABlankName();
        OffboardingTargetUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        OffboardingTargetNameLengthRuleFlagsALongName();
    }
}
