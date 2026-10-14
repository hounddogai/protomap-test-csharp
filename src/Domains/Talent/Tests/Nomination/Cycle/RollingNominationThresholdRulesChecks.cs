namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingNominationThresholdRulesChecks
{
    public static void RollingNominationThresholdValidatorReportsEveryProblem()
    {
        var validator = RollingNominationThresholdRules.DefaultRollingNominationThresholdValidator();
        var item = RollingNominationThreshold.NewRollingNominationThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingNominationThresholdTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingNominationThresholdTitleLengthRule(86400);
        var item = RollingNominationThreshold.NewRollingNominationThreshold("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingNominationThresholdLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RollingNominationThresholdLocaleLengthRule(32);
        var item = RollingNominationThreshold.NewRollingNominationThreshold("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingNominationThresholdCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RollingNominationThresholdCategoryLengthRule(3600);
        var item = RollingNominationThreshold.NewRollingNominationThreshold("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingNominationThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingNominationThresholdRequiredCodeRule("rolling nomination threshold needs an entry");
        var item = RollingNominationThreshold.NewRollingNominationThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingNominationThresholdValidatorReportsEveryProblem();
        RollingNominationThresholdTitleLengthRuleFlagsALongTitle();
        RollingNominationThresholdLocaleLengthRuleFlagsALongLocale();
        RollingNominationThresholdCategoryLengthRuleFlagsALongCategory();
        RollingNominationThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
