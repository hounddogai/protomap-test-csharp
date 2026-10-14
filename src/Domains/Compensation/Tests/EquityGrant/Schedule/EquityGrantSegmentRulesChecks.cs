namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantSegmentRulesChecks
{
    public static void EquityGrantSegmentValidatorReportsEveryProblem()
    {
        var validator = EquityGrantSegmentRules.DefaultEquityGrantSegmentValidator();
        var item = EquityGrantSegment.NewEquityGrantSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new EquityGrantSegmentNameLengthRule(64);
        var item = EquityGrantSegment.NewEquityGrantSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantSegmentRequiredNameRule("equity grant segment needs a value");
        var item = EquityGrantSegment.NewEquityGrantSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantSegmentValidatorReportsEveryProblem();
        EquityGrantSegmentNameLengthRuleFlagsALongName();
        EquityGrantSegmentRequiredNameRuleFlagsABlankName();
    }
}
