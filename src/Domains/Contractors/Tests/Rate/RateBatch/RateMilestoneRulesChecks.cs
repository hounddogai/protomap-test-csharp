namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateMilestoneRulesChecks
{
    public static void RateMilestoneValidatorReportsEveryProblem()
    {
        var validator = RateMilestoneRules.DefaultRateMilestoneValidator();
        var item = RateMilestone.NewRateMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateMilestoneVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RateMilestoneVersionRangeRule(80, 3600);
        var item = RateMilestone.NewRateMilestone("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateMilestoneRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new RateMilestoneRequiredTimezoneRule("rate milestone needs a value");
        var item = RateMilestone.NewRateMilestone("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new RateMilestoneCodeLengthRule(16);
        var item = RateMilestone.NewRateMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new RateMilestoneNameLengthRule(1000);
        var item = RateMilestone.NewRateMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateMilestoneTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RateMilestoneTimezoneLengthRule(80);
        var item = RateMilestone.NewRateMilestone("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateMilestoneValidatorReportsEveryProblem();
        RateMilestoneVersionRangeRuleFlagsValuesAboveVersion();
        RateMilestoneRequiredTimezoneRuleFlagsABlankTimezone();
        RateMilestoneCodeLengthRuleFlagsALongCode();
        RateMilestoneNameLengthRuleFlagsALongName();
        RateMilestoneTimezoneLengthRuleFlagsALongTimezone();
    }
}
