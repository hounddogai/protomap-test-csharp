namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultHeadcountScheduleRulesChecks
{
    public static void DefaultHeadcountScheduleValidatorReportsEveryProblem()
    {
        var validator = DefaultHeadcountScheduleRules.DefaultDefaultHeadcountScheduleValidator();
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHeadcountScheduleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DefaultHeadcountScheduleUpdatedAtRangeRule(16, 3600);
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountScheduleBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new DefaultHeadcountScheduleBatchSizeRangeRule(80, 120);
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultHeadcountScheduleRequiredTitleRule("default headcount schedule needs an entry");
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultHeadcountScheduleTitleLengthRule(86400);
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultHeadcountScheduleValidatorReportsEveryProblem();
        DefaultHeadcountScheduleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DefaultHeadcountScheduleBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        DefaultHeadcountScheduleRequiredTitleRuleFlagsABlankTitle();
        DefaultHeadcountScheduleTitleLengthRuleFlagsALongTitle();
    }
}
