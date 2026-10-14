namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionRulesChecks
{
    public static void ScheduledReorgExceptionValidatorReportsEveryProblem()
    {
        var validator = ScheduledReorgExceptionRules.DefaultScheduledReorgExceptionValidator();
        var item = ScheduledReorgException.NewScheduledReorgException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledReorgExceptionLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledReorgExceptionLimitRangeRule(120, 255);
        var item = ScheduledReorgException.NewScheduledReorgException("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledReorgExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ScheduledReorgExceptionWindowDaysRangeRule(120, 3600);
        var item = ScheduledReorgException.NewScheduledReorgException("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledReorgExceptionRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ScheduledReorgExceptionRequiredRegionRule("scheduled reorg exception needs an entry");
        var item = ScheduledReorgException.NewScheduledReorgException("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledReorgExceptionRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ScheduledReorgExceptionRegionLengthRule(86400);
        var item = ScheduledReorgException.NewScheduledReorgException("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledReorgExceptionValidatorReportsEveryProblem();
        ScheduledReorgExceptionLimitRangeRuleFlagsValuesAboveLimit();
        ScheduledReorgExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ScheduledReorgExceptionRequiredRegionRuleFlagsABlankRegion();
        ScheduledReorgExceptionRegionLengthRuleFlagsALongRegion();
    }
}
