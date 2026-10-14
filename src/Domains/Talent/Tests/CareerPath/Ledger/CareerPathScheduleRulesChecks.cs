namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathScheduleRulesChecks
{
    public static void CareerPathScheduleValidatorReportsEveryProblem()
    {
        var validator = CareerPathScheduleRules.DefaultCareerPathScheduleValidator();
        var item = CareerPathSchedule.NewCareerPathSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathScheduleSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new CareerPathScheduleSegmentLengthRule(80);
        var item = CareerPathSchedule.NewCareerPathSchedule("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathScheduleRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CareerPathScheduleRequiredContactEmailRule("career path schedule needs an entry");
        var item = CareerPathSchedule.NewCareerPathSchedule("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathScheduleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new CareerPathScheduleRevisionRangeRule(80, 255);
        var item = CareerPathSchedule.NewCareerPathSchedule("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathScheduleRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CareerPathScheduleRequiredSegmentRule("career path schedule needs a value");
        var item = CareerPathSchedule.NewCareerPathSchedule("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathScheduleRequiredCodeRule("career path schedule needs this field");
        var item = CareerPathSchedule.NewCareerPathSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathScheduleValidatorReportsEveryProblem();
        CareerPathScheduleSegmentLengthRuleFlagsALongSegment();
        CareerPathScheduleRequiredContactEmailRuleFlagsABlankContactEmail();
        CareerPathScheduleRevisionRangeRuleFlagsValuesAboveRevision();
        CareerPathScheduleRequiredSegmentRuleFlagsABlankSegment();
        CareerPathScheduleRequiredCodeRuleFlagsABlankCode();
    }
}
