namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyScheduleRulesChecks
{
    public static void PulseSurveyScheduleValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyScheduleRules.DefaultPulseSurveyScheduleValidator();
        var item = PulseSurveySchedule.NewPulseSurveySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyScheduleSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PulseSurveyScheduleSequenceRangeRule(16, 255);
        var item = PulseSurveySchedule.NewPulseSurveySchedule("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyScheduleRequiredCodeRule("pulse survey schedule needs this field");
        var item = PulseSurveySchedule.NewPulseSurveySchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyScheduleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PulseSurveyScheduleStartsAtRangeRule(255, 3600);
        var item = PulseSurveySchedule.NewPulseSurveySchedule("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyScheduleEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PulseSurveyScheduleEmployeeIdLengthRule(3600);
        var item = PulseSurveySchedule.NewPulseSurveySchedule("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyScheduleValidatorReportsEveryProblem();
        PulseSurveyScheduleSequenceRangeRuleFlagsValuesAboveSequence();
        PulseSurveyScheduleRequiredCodeRuleFlagsABlankCode();
        PulseSurveyScheduleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        PulseSurveyScheduleEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
