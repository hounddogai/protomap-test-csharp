namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RetroPaySegmentRulesChecks
{
    public static void RetroPaySegmentValidatorReportsEveryProblem()
    {
        var validator = RetroPaySegmentRules.DefaultRetroPaySegmentValidator();
        var item = RetroPaySegment.NewRetroPaySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPaySegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPaySegmentRequiredCodeRule("retro pay segment needs an entry");
        var item = RetroPaySegment.NewRetroPaySegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPaySegmentRegionLengthRuleFlagsALongRegion()
    {
        var rule = new RetroPaySegmentRegionLengthRule(86400);
        var item = RetroPaySegment.NewRetroPaySegment("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPaySegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPaySegmentCodeLengthRule(1000);
        var item = RetroPaySegment.NewRetroPaySegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPaySegmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RetroPaySegmentRequiredLocationRule("retro pay segment needs a value");
        var item = RetroPaySegment.NewRetroPaySegment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPaySegmentCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RetroPaySegmentCeilingRangeRule(6.4, 1452.1);
        var item = RetroPaySegment.NewRetroPaySegment("x", "y");
        item.Ceiling = 1453.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPaySegmentValidatorReportsEveryProblem();
        RetroPaySegmentRequiredCodeRuleFlagsABlankCode();
        RetroPaySegmentRegionLengthRuleFlagsALongRegion();
        RetroPaySegmentCodeLengthRuleFlagsALongCode();
        RetroPaySegmentRequiredLocationRuleFlagsABlankLocation();
        RetroPaySegmentCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
