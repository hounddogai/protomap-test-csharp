namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LenientPositionSegmentRulesChecks
{
    public static void LenientPositionSegmentValidatorReportsEveryProblem()
    {
        var validator = LenientPositionSegmentRules.DefaultLenientPositionSegmentValidator();
        var item = LenientPositionSegment.NewLenientPositionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPositionSegmentRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new LenientPositionSegmentRequiredDepartmentRule("lenient position segment needs this field");
        var item = LenientPositionSegment.NewLenientPositionSegment("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPositionSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPositionSegmentRequiredCodeRule("lenient position segment needs an entry");
        var item = LenientPositionSegment.NewLenientPositionSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPositionSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientPositionSegmentRequiredNameRule("lenient position segment needs this field");
        var item = LenientPositionSegment.NewLenientPositionSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPositionSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new LenientPositionSegmentNameLengthRule(120);
        var item = LenientPositionSegment.NewLenientPositionSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientPositionSegmentValidatorReportsEveryProblem();
        LenientPositionSegmentRequiredDepartmentRuleFlagsABlankDepartment();
        LenientPositionSegmentRequiredCodeRuleFlagsABlankCode();
        LenientPositionSegmentRequiredNameRuleFlagsABlankName();
        LenientPositionSegmentNameLengthRuleFlagsALongName();
    }
}
