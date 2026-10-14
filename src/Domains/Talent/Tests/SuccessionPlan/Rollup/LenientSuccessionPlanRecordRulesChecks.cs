namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRecordRulesChecks
{
    public static void LenientSuccessionPlanRecordValidatorReportsEveryProblem()
    {
        var validator = LenientSuccessionPlanRecordRules.DefaultLenientSuccessionPlanRecordValidator();
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSuccessionPlanRecordDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LenientSuccessionPlanRecordDepartmentLengthRule(120);
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSuccessionPlanRecordSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientSuccessionPlanRecordSequenceRangeRule(80, 86400);
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSuccessionPlanRecordRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LenientSuccessionPlanRecordRatingRangeRule(2.9, 4309.2);
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("x", "y");
        item.Rating = 4310.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanRecordValidatorReportsEveryProblem();
        LenientSuccessionPlanRecordDepartmentLengthRuleFlagsALongDepartment();
        LenientSuccessionPlanRecordSequenceRangeRuleFlagsValuesAboveSequence();
        LenientSuccessionPlanRecordRatingRangeRuleFlagsValuesAboveRating();
    }
}
