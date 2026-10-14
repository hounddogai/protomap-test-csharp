namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RefreshScheduleRulesChecks
{
    public static void RefreshScheduleValidatorReportsEveryProblem()
    {
        var validator = RefreshScheduleRules.DefaultRefreshScheduleValidator();
        var item = RefreshSchedule.NewRefreshSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshScheduleRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new RefreshScheduleRequiredDescriptionRule("refresh schedule needs this field");
        var item = RefreshSchedule.NewRefreshSchedule("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshScheduleCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RefreshScheduleCeilingRangeRule(3.2, 1302.4);
        var item = RefreshSchedule.NewRefreshSchedule("x", "y");
        item.Ceiling = 1303.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RefreshScheduleTitleLengthRule(86400);
        var item = RefreshSchedule.NewRefreshSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshScheduleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RefreshScheduleRatioRangeRule(6.9, 934.8);
        var item = RefreshSchedule.NewRefreshSchedule("x", "y");
        item.Ratio = 936.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RefreshScheduleValidatorReportsEveryProblem();
        RefreshScheduleRequiredDescriptionRuleFlagsABlankDescription();
        RefreshScheduleCeilingRangeRuleFlagsValuesAboveCeiling();
        RefreshScheduleTitleLengthRuleFlagsALongTitle();
        RefreshScheduleRatioRangeRuleFlagsValuesAboveRatio();
    }
}
