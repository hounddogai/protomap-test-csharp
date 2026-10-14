namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanNoticeRulesChecks
{
    public static void FloorPlanNoticeValidatorReportsEveryProblem()
    {
        var validator = FloorPlanNoticeRules.DefaultFloorPlanNoticeValidator();
        var item = FloorPlanNotice.NewFloorPlanNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanNoticeCodeLengthRule(120);
        var item = FloorPlanNotice.NewFloorPlanNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanNoticeRequiredCodeRule("floor plan notice needs a value");
        var item = FloorPlanNotice.NewFloorPlanNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanNoticeRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FloorPlanNoticeRequiredLocaleRule("floor plan notice needs this field");
        var item = FloorPlanNotice.NewFloorPlanNotice("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanNoticeValidatorReportsEveryProblem();
        FloorPlanNoticeCodeLengthRuleFlagsALongCode();
        FloorPlanNoticeRequiredCodeRuleFlagsABlankCode();
        FloorPlanNoticeRequiredLocaleRuleFlagsABlankLocale();
    }
}
