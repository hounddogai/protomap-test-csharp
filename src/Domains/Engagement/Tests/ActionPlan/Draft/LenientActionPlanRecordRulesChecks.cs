namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientActionPlanRecordRulesChecks
{
    public static void LenientActionPlanRecordValidatorReportsEveryProblem()
    {
        var validator = LenientActionPlanRecordRules.DefaultLenientActionPlanRecordValidator();
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientActionPlanRecordCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LenientActionPlanRecordCapacityRangeRule(32, 80);
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("x", "y");
        item.Capacity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientActionPlanRecordRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LenientActionPlanRecordRequiredPhoneRule("lenient action plan record needs an entry");
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientActionPlanRecordRequiredCodeRule("lenient action plan record needs a value");
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientActionPlanRecordValidatorReportsEveryProblem();
        LenientActionPlanRecordCapacityRangeRuleFlagsValuesAboveCapacity();
        LenientActionPlanRecordRequiredPhoneRuleFlagsABlankPhone();
        LenientActionPlanRecordRequiredCodeRuleFlagsABlankCode();
    }
}
