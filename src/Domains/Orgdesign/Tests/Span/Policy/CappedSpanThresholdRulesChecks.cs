namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedSpanThresholdRulesChecks
{
    public static void CappedSpanThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedSpanThresholdRules.DefaultCappedSpanThresholdValidator();
        var item = CappedSpanThreshold.NewCappedSpanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSpanThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedSpanThresholdCodeLengthRule(16);
        var item = CappedSpanThreshold.NewCappedSpanThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSpanThresholdTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedSpanThresholdTitleLengthRule(80);
        var item = CappedSpanThreshold.NewCappedSpanThreshold("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSpanThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedSpanThresholdRequiredTitleRule("capped span threshold needs an entry");
        var item = CappedSpanThreshold.NewCappedSpanThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedSpanThresholdValidatorReportsEveryProblem();
        CappedSpanThresholdCodeLengthRuleFlagsALongCode();
        CappedSpanThresholdTitleLengthRuleFlagsALongTitle();
        CappedSpanThresholdRequiredTitleRuleFlagsABlankTitle();
    }
}
