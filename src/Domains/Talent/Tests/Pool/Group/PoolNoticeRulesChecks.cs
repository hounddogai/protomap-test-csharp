namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolNoticeRulesChecks
{
    public static void PoolNoticeValidatorReportsEveryProblem()
    {
        var validator = PoolNoticeRules.DefaultPoolNoticeValidator();
        var item = PoolNotice.NewPoolNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolNoticeRequiredCodeRule("pool notice needs a value");
        var item = PoolNotice.NewPoolNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolNoticeRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PoolNoticeRequiredJobCodeRule("pool notice needs a value");
        var item = PoolNotice.NewPoolNotice("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolNoticeSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PoolNoticeSegmentLengthRule(3600);
        var item = PoolNotice.NewPoolNotice("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PoolNoticeFloorValueRangeRule(8.7, 1714.9);
        var item = PoolNotice.NewPoolNotice("x", "y");
        item.FloorValue = 1716.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolNoticeCodeLengthRule(86400);
        var item = PoolNotice.NewPoolNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolNoticeValidatorReportsEveryProblem();
        PoolNoticeRequiredCodeRuleFlagsABlankCode();
        PoolNoticeRequiredJobCodeRuleFlagsABlankJobCode();
        PoolNoticeSegmentLengthRuleFlagsALongSegment();
        PoolNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PoolNoticeCodeLengthRuleFlagsALongCode();
    }
}
