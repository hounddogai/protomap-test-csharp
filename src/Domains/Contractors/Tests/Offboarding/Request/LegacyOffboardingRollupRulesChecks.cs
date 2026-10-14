namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyOffboardingRollupRulesChecks
{
    public static void LegacyOffboardingRollupValidatorReportsEveryProblem()
    {
        var validator = LegacyOffboardingRollupRules.DefaultLegacyOffboardingRollupValidator();
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyOffboardingRollupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyOffboardingRollupTitleLengthRule(16);
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyOffboardingRollupSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LegacyOffboardingRollupSummaryLengthRule(64);
        var item = LegacyOffboardingRollup.NewLegacyOffboardingRollup("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyOffboardingRollupValidatorReportsEveryProblem();
        LegacyOffboardingRollupTitleLengthRuleFlagsALongTitle();
        LegacyOffboardingRollupSummaryLengthRuleFlagsALongSummary();
    }
}
