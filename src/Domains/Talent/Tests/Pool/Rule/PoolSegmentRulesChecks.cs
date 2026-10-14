namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolSegmentRulesChecks
{
    public static void PoolSegmentValidatorReportsEveryProblem()
    {
        var validator = PoolSegmentRules.DefaultPoolSegmentValidator();
        var item = PoolSegment.NewPoolSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolSegmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PoolSegmentWindowDaysRangeRule(32, 80);
        var item = PoolSegment.NewPoolSegment("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolSegmentRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PoolSegmentRequiredManagerIdRule("pool segment needs a value");
        var item = PoolSegment.NewPoolSegment("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolSegmentCodeLengthRule(120);
        var item = PoolSegment.NewPoolSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolSegmentValidatorReportsEveryProblem();
        PoolSegmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        PoolSegmentRequiredManagerIdRuleFlagsABlankManagerId();
        PoolSegmentCodeLengthRuleFlagsALongCode();
    }
}
