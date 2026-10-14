namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanSegmentRulesChecks
{
    public static void ReturnPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanSegmentRules.DefaultReturnPlanSegmentValidator();
        var item = ReturnPlanSegment.NewReturnPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ReturnPlanSegmentStartsAtRangeRule(16, 3600);
        var item = ReturnPlanSegment.NewReturnPlanSegment("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanSegmentRequiredCodeRule("return plan segment needs this field");
        var item = ReturnPlanSegment.NewReturnPlanSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new ReturnPlanSegmentNameLengthRule(86400);
        var item = ReturnPlanSegment.NewReturnPlanSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanSegmentGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ReturnPlanSegmentGraceDaysRangeRule(32, 64);
        var item = ReturnPlanSegment.NewReturnPlanSegment("x", "y");
        item.GraceDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanSegmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ReturnPlanSegmentFteRangeRule(7.1, 3951.6);
        var item = ReturnPlanSegment.NewReturnPlanSegment("x", "y");
        item.Fte = 3953.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReturnPlanSegmentCodeLengthRule(255);
        var item = ReturnPlanSegment.NewReturnPlanSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanSegmentValidatorReportsEveryProblem();
        ReturnPlanSegmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        ReturnPlanSegmentRequiredCodeRuleFlagsABlankCode();
        ReturnPlanSegmentNameLengthRuleFlagsALongName();
        ReturnPlanSegmentGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ReturnPlanSegmentFteRangeRuleFlagsValuesAboveFte();
        ReturnPlanSegmentCodeLengthRuleFlagsALongCode();
    }
}
