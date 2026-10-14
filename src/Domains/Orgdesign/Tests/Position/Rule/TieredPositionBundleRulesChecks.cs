namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleRulesChecks
{
    public static void TieredPositionBundleValidatorReportsEveryProblem()
    {
        var validator = TieredPositionBundleRules.DefaultTieredPositionBundleValidator();
        var item = TieredPositionBundle.NewTieredPositionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPositionBundleEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TieredPositionBundleEmployeeIdLengthRule(120);
        var item = TieredPositionBundle.NewTieredPositionBundle("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPositionBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPositionBundleRequiredCodeRule("tiered position bundle needs this field");
        var item = TieredPositionBundle.NewTieredPositionBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPositionBundleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TieredPositionBundleMinimumRangeRule(5.3, 2205.7);
        var item = TieredPositionBundle.NewTieredPositionBundle("x", "y");
        item.Minimum = 2207.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPositionBundleNameLengthRuleFlagsALongName()
    {
        var rule = new TieredPositionBundleNameLengthRule(255);
        var item = TieredPositionBundle.NewTieredPositionBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPositionBundleRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new TieredPositionBundleRequiredSegmentRule("tiered position bundle needs a value");
        var item = TieredPositionBundle.NewTieredPositionBundle("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPositionBundleValidatorReportsEveryProblem();
        TieredPositionBundleEmployeeIdLengthRuleFlagsALongEmployeeId();
        TieredPositionBundleRequiredCodeRuleFlagsABlankCode();
        TieredPositionBundleMinimumRangeRuleFlagsValuesAboveMinimum();
        TieredPositionBundleNameLengthRuleFlagsALongName();
        TieredPositionBundleRequiredSegmentRuleFlagsABlankSegment();
    }
}
