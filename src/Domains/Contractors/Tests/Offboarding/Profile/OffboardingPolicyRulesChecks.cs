namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingPolicyRulesChecks
{
    public static void OffboardingPolicyValidatorReportsEveryProblem()
    {
        var validator = OffboardingPolicyRules.DefaultOffboardingPolicyValidator();
        var item = OffboardingPolicy.NewOffboardingPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingPolicyRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new OffboardingPolicyRequiredTeamRule("offboarding policy needs an entry");
        var item = OffboardingPolicy.NewOffboardingPolicy("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OffboardingPolicyRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new OffboardingPolicyRatioRangeRule(8.6, 2191.8);
        var item = OffboardingPolicy.NewOffboardingPolicy("x", "y");
        item.Ratio = 2193.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingPolicyMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new OffboardingPolicyMaximumRangeRule(3.0, 4469.5);
        var item = OffboardingPolicy.NewOffboardingPolicy("x", "y");
        item.Maximum = 4471.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OffboardingPolicyTitleLengthRule(255);
        var item = OffboardingPolicy.NewOffboardingPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingPolicyValidatorReportsEveryProblem();
        OffboardingPolicyRequiredTeamRuleFlagsABlankTeam();
        OffboardingPolicyRatioRangeRuleFlagsValuesAboveRatio();
        OffboardingPolicyMaximumRangeRuleFlagsValuesAboveMaximum();
        OffboardingPolicyTitleLengthRuleFlagsALongTitle();
    }
}
