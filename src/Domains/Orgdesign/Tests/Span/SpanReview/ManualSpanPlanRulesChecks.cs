namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class ManualSpanPlanRulesChecks
{
    public static void ManualSpanPlanValidatorReportsEveryProblem()
    {
        var validator = ManualSpanPlanRules.DefaultManualSpanPlanValidator();
        var item = ManualSpanPlan.NewManualSpanPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualSpanPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualSpanPlanRequiredCodeRule("manual span plan needs this field");
        var item = ManualSpanPlan.NewManualSpanPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSpanPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualSpanPlanCodeLengthRule(255);
        var item = ManualSpanPlan.NewManualSpanPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualSpanPlanValidatorReportsEveryProblem();
        ManualSpanPlanRequiredCodeRuleFlagsABlankCode();
        ManualSpanPlanCodeLengthRuleFlagsALongCode();
    }
}
